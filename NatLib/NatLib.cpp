// This is the main DLL file.

#include "Stdafx.h"

#include "NatLib.h"

array<System::Drawing::Rectangle>^ NatLib::Nat::detectFaces(void* imgRGB, int w, int h, int stride)
{
	if (!natNat)
		return nullptr;

	std::vector<cv::Rect> f1 = natNat->detectFaces(imgRGB, w, h, stride);
	array<System::Drawing::Rectangle>^ f2 = gcnew array<System::Drawing::Rectangle>(f1.size());
	for (size_t i = 0; i < f1.size(); ++i)
	{
		f2[i].X = f1[i].x;
		f2[i].Y = f1[i].y;
		f2[i].Width = f1[i].width;
		f2[i].Height = f1[i].height;
	}
	return f2;
}