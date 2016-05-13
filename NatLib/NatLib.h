// NatLib.h

#pragma once

#include "NatNat.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace NatLib
{
	public ref class Nat
	{
		NatNat* natNat;

		void destruct()
		{
			NatNat* nn = natNat;
			natNat = nullptr;
			delete nn;
		}

	public:
		Nat() { natNat = new NatNat; }
		~Nat() { destruct(); }
		!Nat() { destruct(); }
		array<Drawing::Rectangle>^ detectFaces(void* imgRGB, int w, int h, int stride, [Out]array<Drawing::Rectangle>^% retEyes);
	};
}
