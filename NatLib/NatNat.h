#pragma once

#pragma managed(push, off)

#include <cstdint>
#include <vector>
#include <opencv2/objdetect/objdetect.hpp>

class NatNat
{
	cv::CascadeClassifier cascade;
public:
	NatNat()
	{
		cascade.load("haarcascades\\haarcascade_frontalface_alt2.xml");
	}
	std::vector<cv::Rect> detectFaces(void* imgRGB, int w, int h, int stride);
	cv::Mat grayImage(void* imgRGB, int w, int h, int stride);
};

#pragma managed(pop)
