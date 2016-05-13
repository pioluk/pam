// This is the main DLL file.

#include "Stdafx.h"

#include "NatLib.h"

array<System::Drawing::Rectangle>^ NatLib::Nat::detectFaces(void* imgRGB, int w, int h, int stride, [Out]array<Drawing::Rectangle>^% retEyes)
{
	std::vector<cv::Rect> e1;
	std::vector<cv::Rect> f1 = natNat->detectFaces(imgRGB, w, h, stride, e1);
	array<Drawing::Rectangle>^ f2 = gcnew array<Drawing::Rectangle>(f1.size());
	for (size_t i = 0; i < f1.size(); ++i)
	{
		f2[i].X = f1[i].x;
		f2[i].Y = f1[i].y;
		f2[i].Width = f1[i].width;
		f2[i].Height = f1[i].height;
	}
	array<Drawing::Rectangle>^ e2 = gcnew array<Drawing::Rectangle>(e1.size());
	for (size_t i = 0; i < e1.size(); ++i)
	{
		e2[i].X = e1[i].x;
		e2[i].Y = e1[i].y;
		e2[i].Width = e1[i].width;
		e2[i].Height = e1[i].height;
	}
	retEyes = e2;
	return f2;
}