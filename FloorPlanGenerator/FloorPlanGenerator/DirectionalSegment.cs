using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloorPlanGenerator
{

    class DirectionalSegment
    {
        public int Left;
        public int Top;
        public enumDirection Direction;
        public int GroupID;

        public DirectionalSegment(int inTop, int inLeft, enumDirection inDirection, int inGroupID)
        {
            Left = inLeft;
            Top = inTop;
            Direction = inDirection;
            GroupID = inGroupID;
        }

        public DirectionalSegment IncrementForwards(int amount)
        {
            DirectionalSegment result = new DirectionalSegment(Top, Left, Direction, GroupID);
            switch (Direction)
            {
                case enumDirection.North:
                    result.Top -= amount;
                    break;
                case enumDirection.East:
                    result.Left += amount;
                    break;
                case enumDirection.South:
                    result.Top += amount;
                    break;
                case enumDirection.West:
                    result.Left -= amount;
                    break;
            }
            return result;
        }

        public DirectionalSegment IncrementLeft(int amount)
        {
            DirectionalSegment result = new DirectionalSegment(Top, Left, enumDirection.None, GroupID);
            switch (Direction)
            {
                case enumDirection.North:
                    result.Direction = enumDirection.West;
                    result.Left -= amount;
                    break;
                case enumDirection.East:
                    result.Direction = enumDirection.North;
                    result.Top -= amount;
                    break;
                case enumDirection.South:
                    result.Direction = enumDirection.East;
                    result.Left += amount;
                    break;
                case enumDirection.West:
                    result.Direction = enumDirection.South;
                    result.Top += amount;
                    break;
            }
            return result;
        }

        public DirectionalSegment IncrementRight(int amount)
        {
            DirectionalSegment result = new DirectionalSegment(Top, Left, enumDirection.None, GroupID);
            switch (Direction)
            {
                case enumDirection.North:
                    result.Direction = enumDirection.East;
                    result.Left += amount;
                    break;
                case enumDirection.East:
                    result.Direction = enumDirection.South;
                    result.Top += amount;
                    break;
                case enumDirection.South:
                    result.Direction = enumDirection.West;
                    result.Left -= amount;
                    break;
                case enumDirection.West:
                    result.Direction = enumDirection.North;
                    result.Top--;
                    break;
            }
            return result;
        }
    }
}
