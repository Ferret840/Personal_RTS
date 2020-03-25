#pragma once

#include "stdafx.h"

template <class T>
class Vector3
{
public:
  T X, Y, Z;
  T sqLength;
  Vector3<T>(T _x, T _y, T _z) : X(_x), Y(_y), Z(_z), sqLength(X * X + Y * Y + Z * Z)
  {
  }
  static Vector3<T> right()
  {
    return Vector3<T>(1, 0, 0);
  }
  static Vector3<T> up()
  {
    return Vector3<T>(0, 1, 0);
  }
  static Vector3<T> forward()
  {
    return Vector3<T>(0, 0, 1);
  }
  static Vector3<T> one()
  {
    return Vector3<T>(1, 1, 1);
  }
  static Vector3<T> zero()
  {
    return Vector3<T>(0, 0, 0);
  }
};

template <class T>
class Vector2
{
public:
  T x, y;
  T sqLength;
  Vector2<T>(T _x, T _y) : x(_x), y(_y), sqLength(x*x + y * y)
  {
  }
  //Vector2<T>& operator=(const Vector2<T>& cpy)
  //{
  //  return cpy;
  //}
};

template <typename T>
T clamp(const T& n, const T& lower, const T& upper)
{
  return max(lower, min(n, upper));
}