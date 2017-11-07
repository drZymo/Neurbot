using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Swoc.MathEx;

namespace Swoc
{
    public struct CartesianCoord
    {
        public CartesianCoord(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X;
        public double Y;

        public PolarCoord AsPolorCoord()
        {
            return AsPolorCoord(X, Y);
        }

        public static PolarCoord AsPolorCoord(double x, double y)
        {
            return new PolarCoord(Math.Pow(x * x + y * y, 0.5), Math.Atan2(y, x));
        }

        public static CartesianCoord operator -(CartesianCoord c1, CartesianCoord c2)
        {
            return new CartesianCoord(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static CartesianCoord operator +(CartesianCoord c1, CartesianCoord c2)
        {
            return new CartesianCoord(c1.X + c2.X, c1.Y + c2.Y);
        }

        public double LengthSquared()
        {
            return X * X + Y * Y;
        }
    }

    public struct PolarCoord
    {
        public enum AngleType
        {
            Degree,
            Radian,
        }

        public PolarCoord(double radius, double angle, AngleType type = AngleType.Radian)
        {
            Radius = radius;
            AngleRadian = type == AngleType.Degree ? DegreeToRadian(angle) : angle;
        }

        double Radius;
        double AngleRadian;

        public double AngleDegree { get { return RadianToDegree(AngleRadian); } }

        public CartesianCoord AsCartesianCoord()
        {
            return AsCartesianCoord(Radius, AngleRadian, AngleType.Radian);
        }

        public static CartesianCoord AsCartesianCoord(double radius, double angle, AngleType type = AngleType.Radian)
        {
            var angleRadian = type == AngleType.Degree ? DegreeToRadian(angle) : angle;
            return new CartesianCoord(radius * Math.Cos(angleRadian), -radius * Math.Sin(angleRadian));
        }
    }
}
