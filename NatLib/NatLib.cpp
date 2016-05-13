// This is the main DLL file.

#include "Stdafx.h"

#include "NatLib.h"

inline Drawing::Rectangle rect_cv2net(const cv::Rect& rc)
{
	return Drawing::Rectangle(rc.x, rc.y, rc.width, rc.height);
}

array<Drawing::Rectangle>^ NatLib::Nat::detectFaces(void* imgRGB, int w, int h, int stride, [Out]array<Drawing::Rectangle>^% retEyes)
{
	std::vector<cv::Rect> e1;
	std::vector<cv::Rect> f1 = natNat->detectFaces(imgRGB, w, h, stride, e1);
	array<Drawing::Rectangle>^ f2 = gcnew array<Drawing::Rectangle>(f1.size());
	for (size_t i = 0; i < f1.size(); ++i)
		f2[i] = rect_cv2net(f1[i]);
	array<Drawing::Rectangle>^ e2 = gcnew array<Drawing::Rectangle>(e1.size());
	for (size_t i = 0; i < e1.size(); ++i)
		e2[i] = rect_cv2net(e1[i]);
	retEyes = e2;
	return f2;
}