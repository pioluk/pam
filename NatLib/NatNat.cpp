#include "Stdafx.h"

#pragma managed(push, off)

#include <Windows.h>

#include "NatNat.h"

#include <opencv2/imgproc/imgproc.hpp>

HMODULE g_hThisDll = NULL;

BOOL WINAPI DllMain(HINSTANCE hDll, DWORD dwReason, LPVOID reserved)
{
	if (dwReason == DLL_PROCESS_ATTACH)
		g_hThisDll = hDll;
	return TRUE;
}

std::vector<cv::Rect> NatNat::detectFaces(void* imgRGB, int w, int h, int stride, std::vector<cv::Rect>& retEyes)
{
	cv::Mat imgGray = grayImage(imgRGB, w, h, stride);
	cv::equalizeHist(imgGray, imgGray);
	std::vector<cv::Rect> faceRectsBuf;
	cscFace.detectMultiScale(imgGray, faceRectsBuf, 1.1, 3, 0, cv::Size(96, 96), cv::Size());
	cscEye.detectMultiScale(imgGray, retEyes);
	return faceRectsBuf;
}

cv::Mat NatNat::grayImage(void* imgRGB, int w, int h, int stride)
{
	cv::Mat gray{ h, w, CV_8UC1 };
	size_t grStep = gray.step1();
	uchar* pGrLine = gray.data;
	uint8_t* pLine = (uint8_t*)imgRGB;
	for (int y = 0; y < h; ++y)
	{
		uint8_t* pPix = pLine;
		uchar* pGrPix = pGrLine;
		for (int x = 0; x < w; ++x)
		{
			int s = 0;
			for (int i = 0; i < 3; ++i)
				s += pPix[i];
			*pGrPix = uchar(s / 3);
			pPix += 3;
			++pGrPix;
		}
		pLine += stride;
		pGrLine += grStep;
	}
	return gray;
}

#pragma managed(pop)
