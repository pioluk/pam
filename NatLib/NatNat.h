#pragma once

#pragma managed(push, off)

#include <cstdint>
#include <vector>
#include <opencv2/objdetect/objdetect.hpp>

class NatNat
{
	cv::CascadeClassifier cscFace;
	cv::CascadeClassifier cscEye;
public:
	NatNat()
	{
		cscFace.load("haarcascades\\haarcascade_frontalface_default.xml");
		cscEye.load("haarcascades\\haarcascade_eye.xml");
	}
	std::vector<cv::Rect> detectFaces(void* imgRGB, int w, int h, int stride, std::vector<cv::Rect>& retEyes, std::vector<cv::Rect>& retFaceEyes);
	cv::Mat grayImage(void* imgRGB, int w, int h, int stride);
};

#pragma managed(pop)
