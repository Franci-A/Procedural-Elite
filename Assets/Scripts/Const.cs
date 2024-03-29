﻿using System;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public static class Utils {	

    public enum ORIENTATION
    {
        NONE = 0,
        NORTH = (1 << 0),
        EAST = (1 << 1),
        SOUTH = (1 << 2),
        WEST = (1 << 3),
    }

    /// <summary>
    /// Transforms an ORIENTATION into angle
    /// </summary>
    public static float OrientationToAngle(ORIENTATION orientation, ORIENTATION origin = ORIENTATION.NORTH)
    {
        float toNorthAngle = 0;
        switch (orientation) {
            case ORIENTATION.NORTH: toNorthAngle = 0.0f; break;
            case ORIENTATION.EAST: toNorthAngle = 90.0f; break;
            case ORIENTATION.SOUTH: toNorthAngle = 180.0f; break;
            case ORIENTATION.WEST: toNorthAngle = 270.0f; break;
            default: toNorthAngle = 270.0f; break;
        }
        if (origin == ORIENTATION.NORTH) {
            return toNorthAngle;
        }
        float originToNorthAngle = OrientationToAngle(origin, ORIENTATION.NORTH);
        return Mathf.Repeat(toNorthAngle - originToNorthAngle, 360.0f);
    }

    /// <summary>
    /// Transforms an angle into ORIENTATION
    /// </summary>
    public static ORIENTATION AngleToOrientation(float angle, ORIENTATION origin = ORIENTATION.NORTH)
    {
        int roundAngle = (int)Mathf.Round(angle / 90.0f);
        switch (origin)
        {
            case ORIENTATION.NORTH: roundAngle += 0; break;
            case ORIENTATION.EAST: roundAngle += 1; break;
            case ORIENTATION.SOUTH: roundAngle += 2; break;
            case ORIENTATION.WEST: roundAngle += 3; break;
        }
        roundAngle = roundAngle % 4;
        if(roundAngle < 0)
        {
            roundAngle += 4;
        }
        switch(roundAngle)
        {
            case 0: return ORIENTATION.NORTH;
            case 1: return ORIENTATION.EAST;
            case 2: return ORIENTATION.SOUTH;
            case 3: return ORIENTATION.WEST;
            default: return ORIENTATION.NONE;
        }
    }

	/// <summary>
    /// Transforms an ORIENTATION into direction (Vector2Int)
    /// </summary>
	public static Vector2Int OrientationToDir(ORIENTATION orientation)
	{
		switch (orientation)
		{
			case ORIENTATION.NORTH: return new Vector2Int(0,1);
			case ORIENTATION.EAST: return new Vector2Int(1, 0);
			case ORIENTATION.SOUTH: return new Vector2Int(0, -1);
			case ORIENTATION.WEST: return new Vector2Int(-1, 0);
			default: return new Vector2Int(0, 0);
		}
	}

    /// <summary>
    /// Transforms a direction into ORIENTATION (Vector2Int)
    /// </summary>
	public static ORIENTATION DirToOrientation(Vector2 dir)
    {
        if (dir == new Vector2(0, 1)) return ORIENTATION.NORTH;
        else if (dir == new Vector2(1, 0)) return ORIENTATION.EAST;
        else if (dir == new Vector2(0, -1)) return ORIENTATION.SOUTH;
        else if (dir == new Vector2(-1, 0)) return ORIENTATION.WEST;
        else return ORIENTATION.NONE;
    }

    /// <summary>
    /// Gets opposit orientation for a given orientation
    /// </summary>
	public static ORIENTATION OppositeOrientation(ORIENTATION orientation)
	{
		switch (orientation)
		{
			case ORIENTATION.NORTH: return ORIENTATION.SOUTH;
			case ORIENTATION.EAST: return ORIENTATION.WEST;
			case ORIENTATION.SOUTH: return ORIENTATION.NORTH;
			case ORIENTATION.WEST: return ORIENTATION.EAST;
			default: return ORIENTATION.NONE;
		}
	}

	/// <summary>
    /// Transforms an angle into a discrete angle.
    /// </summary>
	public static float DiscreteAngle(float angle, float step)
    {
        return Mathf.Round(angle / step) * step;
    }

    public static ORIENTATION GetRandomOrientation()
    {
        int value = Random.Range(0, 4);
        switch (value)
        {
            case 0: return ORIENTATION.NORTH;
            case 1: return ORIENTATION.EAST;
            case 2: return ORIENTATION.SOUTH;
            default: return ORIENTATION.WEST;
        }
    }

    public static ORIENTATION GetRandomOrientation(params ORIENTATION[] exception)
    {
        if (exception.Length >= 4)
            throw new Exception("No Orientation Available. Too many exceptions");
        else if (exception.Length == 3)
        {
            var enumList = (ORIENTATION[])Enum.GetValues(typeof(ORIENTATION));
            return enumList.First(o => o != ORIENTATION.NONE && !exception.Contains(o));
        }

        ORIENTATION orientation = GetRandomOrientation();
        while(exception.Contains(orientation))
        {
            orientation = GetRandomOrientation();
        }
        return orientation;
    }

}
