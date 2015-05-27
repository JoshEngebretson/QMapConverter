using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;
using QMapConverter.Util;

namespace QMapConverter
{
    /// <summary>
    /// Toe.Math bounding box sucks
    /// </summary>
    public class BoundingBox
    {
        public Vector3 min = new Vector3();
	    public Vector3 max = new Vector3();

	    private Vector3 cnt = new Vector3();
	    private Vector3 dim = new Vector3();

	   
	    public Vector3 Center {
            get {
                return cnt;
            }
        }

	    public Vector3 Dim {
            get { return dim;}
	    }

	    public float getWidth () {
		    return dim.X;
	    }

	    public float GetHeight() {
		    return dim.Y;
	    }

	    public float GetDepth() {
		    return dim.Z;
	    }

	    public Vector3 GetMin() {
		    return new Vector3(min);
	    }

	    public Vector3 getMax () {
		    return new Vector3(max);
	    }

	    /** Constructs a new bounding box with the minimum and maximum vector Set to zeros. */
	    public BoundingBox () {
		    clr();
	    }

	    /** Constructs a new bounding box from the given bounding box.
	     * 
	     * @param bounds The bounding box to copy */
	    public BoundingBox (BoundingBox bounds) {
		    this.Set(bounds);
	    }

	    /** Constructs the new bounding box using the given minimum and maximum vector.
	     * 
	     * @param minimum The minimum vector
	     * @param maximum The maximum vector */
	    public BoundingBox (Vector3 minimum, Vector3 maximum) {
		    this.Set(minimum, maximum);
	    }

	    /** Sets the given bounding box.
	     * 
	     * @param bounds The bounds.
	     * @return This bounding box for chaining. */
	    public BoundingBox Set (BoundingBox bounds) {
		    return this.Set(bounds.min, bounds.max);
	    }

	    /** Sets the given minimum and maximum vector.
	     * 
	     * @param minimum The minimum vector
	     * @param maximum The maximum vector
	     * @return This bounding box for chaining. */
	    public BoundingBox Set (Vector3 minimum, Vector3 maximum) {
		    min = min.Set(minimum.X < maximum.Y ? minimum.X : maximum.X, minimum.Y < maximum.Y ? minimum.Y : maximum.Y,
			    minimum.Z < maximum.Z ? minimum.Z : maximum.Z);
		    max = max.Set(minimum.X > maximum.X ? minimum.X : maximum.X, minimum.Y > maximum.Y ? minimum.Y : maximum.Y,
			    minimum.Z > maximum.Z ? minimum.Z : maximum.Z);
		    cnt = cnt.Set(min);
            cnt = Vector3.Multiply(Vector3.Add(cnt, max), 0.5f);
            dim = dim.Set(max);
            dim = Vector3.Subtract(dim, min);
		    return this;
	    }

	    /** Sets the bounding box minimum and maximum vector from the given points.
	     * 
	     * @param points The points.
	     * @return This bounding box for chaining. */
	    public BoundingBox Set (Vector3[] points) {
		    this.inf();
		    foreach (Vector3 l_point in points)
			    this.ext(l_point);
		    return this;
	    }

	    /** Sets the bounding box minimum and maximum vector from the given points.
	     * 
	     * @param points The points.
	     * @return This bounding box for chaining. */
	    public BoundingBox Set (List<Vector3> points) {
		    this.inf();
		    foreach (Vector3 l_point in points)
			    this.ext(l_point);
		    return this;
	    }

	    /** Sets the minimum and maximum vector to positive and negative infinity.
	     * 
	     * @return This bounding box for chaining. */
	    public BoundingBox inf () {
		    min.Set(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		    max.Set(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		    cnt.Set(0, 0, 0);
		    dim.Set(0, 0, 0);
		    return this;
	    }

	    /** Extends the bounding box to incorporate the given {@link Vector3}.
	     * @param point The vector
	     * @return This bounding box for chaining. */
	    public BoundingBox ext (Vector3 point) {
		    return this.Set(
                min.Set(Math.Min(min.X, point.X), Math.Min(min.Y, point.Y), Math.Min(min.Z, point.Z)),
			    max.Set(Math.Max(max.X, point.X), Math.Max(max.Y, point.Y), Math.Max(max.Z, point.Z)));
	    }

	    /** Sets the minimum and maximum vector to zeros.
	     * @return This bounding box for chaining. */
	    public BoundingBox clr () {
		    return this.Set(min.Set(0, 0, 0), max.Set(0, 0, 0));
	    }

	    /** Returns whether this bounding box is valid. This means that {@link #max} is greater than {@link #min}.
	     * @return True in case the bounding box is valid, false otherwise */
	    public bool isValid () {
		    return min.X < max.X && min.Y < max.Y && min.Z < max.Z;
	    }

	    /** Extends this bounding box by the given bounding box.
	     * 
	     * @param a_bounds The bounding box
	     * @return This bounding box for chaining. */
	    public BoundingBox ext (BoundingBox a_bounds) {
            return this.Set(min.Set(Math.Min(min.X, a_bounds.min.X), Math.Min(min.Y, a_bounds.min.Y), Math.Min(min.Z, a_bounds.min.Z)),
                max.Set(Math.Max(max.X, a_bounds.max.X), Math.Max(max.Y, a_bounds.max.Y), Math.Max(max.Z, a_bounds.max.Z)));
	    }

	    /** Returns whether the given bounding box is contained in this bounding box.
	     * @param b The bounding box
	     * @return Whether the given bounding box is contained */
	    public bool contains (BoundingBox b) {
		    return !isValid()
			    || (min.X <= b.min.X && min.Y <= b.min.Y && min.Z <= b.min.Z && max.X >= b.max.X && max.Y >= b.max.Y && max.Z >= b.max.Z);
	    }

	    /** Returns whether the given bounding box is intersecting this bounding box (at least one point in).
	     * @param b The bounding box
	     * @return Whether the given bounding box is intersected */
	    public bool intersects (BoundingBox b) {
		    if (!isValid()) return false;

		    // test using SAT (separating axis theorem)

		    float lx = Math.Abs(this.cnt.X - b.cnt.X);
		    float sumx = (this.dim.X / 2.0f) + (b.dim.X / 2.0f);

		    float ly = Math.Abs(this.cnt.Y - b.cnt.Y);
		    float sumy = (this.dim.Y / 2.0f) + (b.dim.Y / 2.0f);

		    float lz = Math.Abs(this.cnt.Z - b.cnt.Z);
		    float sumz = (this.dim.Z / 2.0f) + (b.dim.Z / 2.0f);

		    return (lx <= sumx && ly <= sumy && lz <= sumz);

	    }

	    /** Returns whether the given vector is contained in this bounding box.
	     * @param v The vector
	     * @return Whether the vector is contained or not. */
	    public bool Contains(Vector3 v) {
		    return min.X <= v.X && max.X >= v.X && min.Y <= v.Y && max.Y >= v.Y && min.Z <= v.Z && max.Z >= v.Z;
	    }

	    public String toString () {
		    return "[" + min + "|" + max + "]";
	    }

	    /** Extends the bounding box by the given vector.
	     * 
	     * @param x The x-coordinate
	     * @param y The y-coordinate
	     * @param z The z-coordinate
	     * @return This bounding box for chaining. */
	    public BoundingBox ext (float x, float y, float z) {
            return this.Set(min.Set(Math.Min(min.X, x), Math.Min(min.Y, y), Math.Min(min.Z, z)), max.Set(Math.Max(max.X, x), Math.Max(max.Y, y), Math.Max(max.Z, z)));
	    }
    }
}
