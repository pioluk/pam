// This is the main DLL file.

#include "Stdafx.h"

#include "NatLib.h"

inline Drawing::Rectangle rect_cv2net(const cv::Rect& rc)
{
	return Drawing::Rectangle(rc.x, rc.y, rc.width, rc.height);
}

inline array<Drawing::Rectangle>^ rect_cvVec2netArr(const std::vector<cv::Rect>& v)
{
	array<Drawing::Rectangle>^ a = gcnew array<Drawing::Rectangle>(v.size());
	for (size_t i = 0; i < v.size(); ++i)
		a[i] = rect_cv2net(v[i]);
	return a;
}

array<Drawing::Rectangle>^ NatLib::Nat::detectFaces(void* imgRGB, int w, int h, int stride, [Out]array<Drawing::Rectangle>^% retEyes, [Out]array<Drawing::Rectangle>^% retFaceEyes)
{
	std::vector<cv::Rect> e1, fe1;
	std::vector<cv::Rect> f1 = natNat->detectFaces(imgRGB, w, h, stride, e1, fe1);
	array<Drawing::Rectangle>^ f2 = rect_cvVec2netArr(f1);
	array<Drawing::Rectangle>^ e2 = rect_cvVec2netArr(e1);
	array<Drawing::Rectangle>^ fe2 = rect_cvVec2netArr(fe1);
	retEyes = e2;
	retFaceEyes = fe2;
	return f2;
}