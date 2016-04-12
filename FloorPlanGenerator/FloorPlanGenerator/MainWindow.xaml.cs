using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Threading;

namespace FloorPlanGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rnd = new Random();
        Dictionary<int, Brush> colorDict = new Dictionary<int, Brush>();
        int fpWidth = 30;
        int fpHeight = 30;
        
        int fpMaxRooms = 800;
        int fpMaxRoomsPerGroup = 15;
        int fpMaxLargeRooms = 15;
        int fpMaxRoomSize = 3;

        int fpBlueprintSections = 4;
        int fpfootprintlength = 4;
        int fpfootprintwidth = 4;
        
        bool doPause = false;
        bool drawRoomNumbers = false;
        bool drawGroupNumbers = false;
        bool drawCorridorGroups = false;

        Thread mainThread;
        FloorPlan fp;
        Queue<PropertyInfo> brushInfoQueue = new Queue<PropertyInfo>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;
            int select = rnd.Next(brushInfoQueue.Count);
            for (int i = 0; i < select; i++) brushInfoQueue.Enqueue(brushInfoQueue.Dequeue());
            if (brushInfoQueue.Count == 0) RefillBrushQueue();
            result = (Brush)brushInfoQueue.Dequeue().GetValue(null, null);
            if (result == Brushes.Black) result = (Brush)brushInfoQueue.Dequeue().GetValue(null, null);
            if (result == Brushes.White) result = (Brush)brushInfoQueue.Dequeue().GetValue(null, null);
            //return Brushes.Gray;
            return result;
        }

        private void RefillBrushQueue()
        {
            Type brushesType = typeof(Brushes);
            PropertyInfo[] properties = brushesType.GetProperties();
            brushInfoQueue = new Queue<PropertyInfo>(properties);

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            RefillBrushQueue();
            fp = new FloorPlan(fpWidth, fpHeight, fpMaxRooms, fpMaxRoomsPerGroup, fpMaxRoomSize, fpMaxLargeRooms, fpBlueprintSections, fpfootprintlength, fpfootprintwidth, doPause);
            try
            {
                mainThread = new Thread(fp.GenerateFloorPlan);
                mainThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Done");
        }

        private void DrawDoors(int tileSize, int hallwaySize)
        {
            // Create a path for drawing the hallways
            Path doorsPath = new Path();
            doorsPath.Stroke = Brushes.Black;
            doorsPath.StrokeThickness = 1;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Colors.Black;
            doorsPath.Fill = mySolidColorBrush;

            // Use a composite geometry for adding the doors to
            CombinedGeometry doorsGeometryGroup = new CombinedGeometry();
            doorsGeometryGroup.GeometryCombineMode = GeometryCombineMode.Union;
            doorsGeometryGroup.Geometry1 = null;
            doorsGeometryGroup.Geometry2 = null;

            for (int top = 0; top < fpHeight; top++)
            {
                for (int left = 0; left < fpWidth; left++)
                {
                    if (fp.floorGrid[top, left] != null)
                    {
                        FloorSegment thisSegment = fp.floorGrid[top, left];
                        double doorX = 0;
                        double doorY = 0;
                        double doorWidth = 0;
                        double doorHeight = 0;
                        // position the doors differently depending on weather or not there's a hallway.  Math, bitches.
                        foreach (enumDirection nextDoorDir in thisSegment.Doors)
                        {
                            switch (nextDoorDir)
                            {
                                case enumDirection.North:
                                    FloorSegment northSegment = fp.IsInBounds(top - 1, left) ? fp.floorGrid[top - 1, left] : null;
                                    doorX = (left * tileSize) + (tileSize / 2) - (hallwaySize / 2);
                                    doorY = northSegment != null ? northSegment.GroupID != thisSegment.GroupID ? (top * tileSize) + (hallwaySize / 4) : (top * tileSize) - (hallwaySize / 4) : (top * tileSize) - (hallwaySize / 4);
                                    doorWidth = hallwaySize;
                                    doorHeight = hallwaySize / 2;
                                    break;
                                case enumDirection.South:
                                    FloorSegment southSegment = fp.IsInBounds(top + 1, left) ? fp.floorGrid[top + 1, left] : null;
                                    doorX = (left * tileSize) + (tileSize / 2) - (hallwaySize / 2);
                                    doorY = southSegment != null ? southSegment.GroupID != thisSegment.GroupID ? ((top * tileSize) + tileSize) - ((hallwaySize / 4) * 3) : ((top * tileSize) + tileSize) - (hallwaySize / 4) : ((top * tileSize) + tileSize) - (hallwaySize / 4);
                                    doorWidth = hallwaySize;
                                    doorHeight = hallwaySize / 2;
                                    break;
                                case enumDirection.East:
                                    FloorSegment eastSegment = fp.IsInBounds(top, left + 1) ? fp.floorGrid[top, left + 1] : null;
                                    doorX = eastSegment != null ? eastSegment.GroupID != thisSegment.GroupID ? ((left * tileSize) + tileSize) - ((hallwaySize / 4) * 3) : ((left * tileSize) + tileSize) - (hallwaySize / 4) : ((left * tileSize) + tileSize) - (hallwaySize / 4);
                                    doorY = (top * tileSize) + (tileSize / 2) - (hallwaySize / 2);
                                    doorWidth = hallwaySize / 2;
                                    doorHeight = hallwaySize;
                                    break;
                                case enumDirection.West:
                                    FloorSegment westSegment = fp.IsInBounds(top, left - 1) ? fp.floorGrid[top, left - 1] : null;
                                    doorX = westSegment != null ? westSegment.GroupID != thisSegment.GroupID ? (left * tileSize) + (hallwaySize / 4) : (left * tileSize) - (hallwaySize / 4) : (left * tileSize) - (hallwaySize / 4);
                                    doorY = (top * tileSize) + (tileSize / 2) - (hallwaySize / 2);
                                    doorWidth = hallwaySize / 2;
                                    doorHeight = hallwaySize;
                                    break;
                            }
                            RectangleGeometry newDoorGeometry = new RectangleGeometry(new Rect(doorX, doorY, doorWidth, doorHeight));
                            doorsGeometryGroup = new CombinedGeometry()
                            {
                                Geometry1 = doorsGeometryGroup,
                                Geometry2 = newDoorGeometry
                            };
                        }
                    }
                }
            }

            doorsPath.Data = doorsGeometryGroup;
            RenderOptions.SetEdgeMode(doorsPath, EdgeMode.Aliased);
            MainCanvas.Children.Add(doorsPath);
        }

        private void DrawHallways(int tileSize, int hallwaySize)
        {
            // Create a path for drawing the hallways
            Path hallwayPath = new Path();
            hallwayPath.Stroke = Brushes.Black;
            hallwayPath.StrokeThickness = 1;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Colors.White;
            hallwayPath.Fill = mySolidColorBrush;

            // Use a composite geometry for adding the hallways to
            CombinedGeometry hallwayGeometryGroup = new CombinedGeometry();
            hallwayGeometryGroup.GeometryCombineMode = GeometryCombineMode.Union;
            hallwayGeometryGroup.Geometry1 = null;
            hallwayGeometryGroup.Geometry2 = null;

            for (int top = 0; top < fpHeight; top++)
            {
                for (int left = 0; left < fpWidth; left++)
                {
                    if (fp.floorGrid[top, left] != null)
                    {
                        FloorSegment thisSegment = fp.floorGrid[top, left];

                        // If the segment above is in bounds and from a different group, we draw a hallway.
                        // Otherwise, if it's a different room of the same group, we draw a wall.

                        FloorSegment northSegment = fp.IsInBounds(top - 1, left) ? fp.floorGrid[top - 1, left] : null;
                        if (northSegment != null)
                        {
                            if (northSegment.GroupID != thisSegment.GroupID)
                            {
                                RectangleGeometry newHallwayGeometry = new RectangleGeometry(
                                    new Rect(
                                        (left * tileSize) - (hallwaySize / 2),
                                        (top * tileSize) - (hallwaySize / 2),
                                        tileSize + hallwaySize,
                                        hallwaySize));
                                hallwayGeometryGroup = new CombinedGeometry()
                                {
                                    Geometry1 = hallwayGeometryGroup,
                                    Geometry2 = newHallwayGeometry
                                };

                            }
                        }

                        FloorSegment southSegment = fp.IsInBounds(top + 1, left) ? fp.floorGrid[top + 1, left] : null;
                        if (southSegment != null)
                        {
                            if (southSegment.GroupID != thisSegment.GroupID)
                            {
                                RectangleGeometry newHallwayGeometry = new RectangleGeometry(
                                    new Rect(
                                        (left * tileSize) - (hallwaySize / 2),
                                        ((top * tileSize) + tileSize) - (hallwaySize / 2),
                                        tileSize + hallwaySize,
                                        hallwaySize));
                                hallwayGeometryGroup = new CombinedGeometry()
                                {
                                    Geometry1 = hallwayGeometryGroup,
                                    Geometry2 = newHallwayGeometry
                                };
                            }
                        }

                        FloorSegment eastSegment = fp.IsInBounds(top, left + 1) ? fp.floorGrid[top, left + 1] : null;
                        if (eastSegment != null)
                        {
                            if (eastSegment.GroupID != thisSegment.GroupID)
                            {
                                RectangleGeometry newHallwayGeometry = new RectangleGeometry(
                                    new Rect(
                                        ((left * tileSize) + tileSize) - (hallwaySize / 2),
                                        (top * tileSize) - (hallwaySize / 2),
                                        hallwaySize,
                                        tileSize + hallwaySize));
                                hallwayGeometryGroup = new CombinedGeometry()
                                {
                                    Geometry1 = hallwayGeometryGroup,
                                    Geometry2 = newHallwayGeometry
                                };
                            }
                        }

                        FloorSegment westSegment = fp.IsInBounds(top, left - 1) ? fp.floorGrid[top, left - 1] : null;
                        if (westSegment != null)
                        {
                            if (westSegment.GroupID != thisSegment.GroupID)
                            {
                                RectangleGeometry newHallwayGeometry = new RectangleGeometry(
                                    new Rect(
                                        (left * tileSize) - (hallwaySize / 2),
                                        (top * tileSize) - (hallwaySize / 2),
                                        hallwaySize,
                                        tileSize + hallwaySize));
                                hallwayGeometryGroup = new CombinedGeometry()
                                {
                                    Geometry1 = hallwayGeometryGroup,
                                    Geometry2 = newHallwayGeometry
                                };
                            }
                        }
                    }
                }
            }
            hallwayPath.Data = hallwayGeometryGroup;
            RenderOptions.SetEdgeMode(hallwayPath, EdgeMode.Aliased);
            MainCanvas.Children.Add(hallwayPath);

            // Draw the remaining eligible segments
            if (fp.pause)
            {
                foreach (DirectionalSegment ds in fp.eligibleFloorSegments)
                {
                    Rectangle newRect = new Rectangle()
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    MainCanvas.Children.Add(newRect);
                    Canvas.SetLeft(newRect, ds.Left * tileSize);
                    Canvas.SetTop(newRect, ds.Top * tileSize);
                }
            }
            // Signal the algorithm to continue
            fp.waitEvent.Set();
            labelCountRoom.Content = fp.currRoomID.ToString();
        }

        private void DrawData(int tileSize)
        {
            // Draw the floorplan
            MainCanvas.Children.Clear();
            for (int top = 0; top < fpHeight; top++)
            {
                for (int left = 0; left < fpWidth; left++)
                {
                    if (fp.floorGrid[top, left] != null)
                    {
                        FloorSegment thisSegment = fp.floorGrid[top, left];
                        if (!colorDict.ContainsKey(thisSegment.GroupID))
                        {
                            colorDict.Add(thisSegment.GroupID, PickBrush());
                        }
                        Rectangle newRect = new Rectangle()
                        {
                            Width = tileSize,
                            Height = tileSize,
                            Fill = colorDict[thisSegment.GroupID]
                        };
                        RenderOptions.SetEdgeMode(newRect, EdgeMode.Aliased);

                        // Draw the lines between squares if they belong to different rooms
                        FloorSegment northSegment = fp.IsInBounds(top - 1, left) ? fp.floorGrid[top - 1, left] : null;
                        bool drawNorthLine = northSegment == null ? true : (northSegment.RoomID != thisSegment.RoomID) ? true : false;
                        if (drawNorthLine)
                        {
                            Line newLine = new Line()
                            {
                                Stroke = Brushes.Black,
                                StrokeThickness = 2,
                                X1 = left * tileSize,
                                Y1 = top * tileSize,
                                X2 = (left * tileSize) + tileSize,
                                Y2 = top * tileSize
                            };
                            MainCanvas.Children.Add(newLine);
                        }

                        FloorSegment westSegment = fp.IsInBounds(top, left - 1) ? fp.floorGrid[top, left - 1] : null;
                        bool drawWestLine = westSegment == null ? true : (westSegment.RoomID != thisSegment.RoomID) ? true : false;
                        if (drawWestLine)
                        {
                            Line newLine = new Line()
                            {
                                Stroke = Brushes.Black,
                                StrokeThickness = 2,
                                X1 = left * tileSize,
                                Y1 = top * tileSize,
                                X2 = left * tileSize,
                                Y2 = (top * tileSize) + tileSize
                            };
                            MainCanvas.Children.Add(newLine);
                        }

                        FloorSegment eastSegment = fp.IsInBounds(top, left + 1) ? fp.floorGrid[top, left + 1] : null;
                        bool drawEastLine = eastSegment == null ? true : (eastSegment.RoomID != thisSegment.RoomID) ? true : false;
                        if (drawEastLine)
                        {
                            Line newLine = new Line()
                            {
                                Stroke = Brushes.Black,
                                StrokeThickness = 2,
                                X1 = (left * tileSize) + tileSize,
                                Y1 = top * tileSize,
                                X2 = (left * tileSize) + tileSize,
                                Y2 = (top * tileSize) + tileSize
                            };
                            MainCanvas.Children.Add(newLine);
                        }

                        FloorSegment southSegment = fp.IsInBounds(top + 1, left) ? fp.floorGrid[top + 1, left] : null;
                        bool drawSouthLine = southSegment == null ? true : (southSegment.RoomID != thisSegment.RoomID) ? true : false;
                        if (drawSouthLine)
                        {
                            Line newLine = new Line()
                            {
                                Stroke = Brushes.Black,
                                StrokeThickness = 2,
                                X1 = left * tileSize,
                                Y1 = (top * tileSize) + tileSize,
                                X2 = (left * tileSize) + tileSize,
                                Y2 = (top * tileSize) + tileSize
                            };
                            MainCanvas.Children.Add(newLine);
                        }
                        MainCanvas.Children.Add(newRect);
                        Canvas.SetLeft(newRect, left * tileSize);
                        Canvas.SetTop(newRect, top * tileSize);

                        if (drawRoomNumbers)
                        {
                            TextBlock tbRoomNum = new TextBlock();
                            tbRoomNum.Inlines.Add(new Run(fp.floorGrid[top, left].RoomID.ToString()));
                            RenderOptions.SetEdgeMode(tbRoomNum, EdgeMode.Aliased);
                            MainCanvas.Children.Add(tbRoomNum);
                            Canvas.SetLeft(tbRoomNum, left * tileSize + 6);
                            Canvas.SetTop(tbRoomNum, top * tileSize + 6);
                        }
                        else if (drawGroupNumbers)
                        {
                            TextBlock tbGroupNum = new TextBlock();
                            tbGroupNum.Inlines.Add(new Run(fp.floorGrid[top, left].GroupID.ToString()));
                            RenderOptions.SetEdgeMode(tbGroupNum, EdgeMode.Aliased);
                            MainCanvas.Children.Add(tbGroupNum);
                            Canvas.SetLeft(tbGroupNum, left * tileSize + 6);
                            Canvas.SetTop(tbGroupNum, top * tileSize + 6);
                        }
                        else if (drawCorridorGroups)
                        {
                            TextBlock tbGroupNum = new TextBlock();
                            HashSet<string> groupValues = fp.floorGrid[top, left].ConnectedCorridorIDs;
                            if(groupValues.Count > 0)
                            {
                                string groupCSV = String.Join(",", groupValues.Select(x => x.ToString()).ToArray());
                                tbGroupNum.Inlines.Add(new Run(groupCSV));
                                RenderOptions.SetEdgeMode(tbGroupNum, EdgeMode.Aliased);
                                MainCanvas.Children.Add(tbGroupNum);
                                Canvas.SetLeft(tbGroupNum, left * tileSize + 6);
                                Canvas.SetTop(tbGroupNum, top * tileSize + 6);
                            }
                        }
                    }
                }
            }

            // Draw the remaining eligible segments
            if (fp.pause)
            {
                foreach (DirectionalSegment ds in fp.eligibleFloorSegments)
                {
                    Rectangle newRect = new Rectangle()
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    MainCanvas.Children.Add(newRect);
                    Canvas.SetLeft(newRect, ds.Left * tileSize);
                    Canvas.SetTop(newRect, ds.Top * tileSize);
                }
            }
            // Signal the algorithm to continue
            fp.waitEvent.Set();
            labelCountRoom.Content = fp.currRoomID.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DrawData(32);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            mainThread.Abort();
            rnd = new Random();
            Type brushesType = typeof(Brushes);
            PropertyInfo[] properties = brushesType.GetProperties();
            brushInfoQueue = new Queue<PropertyInfo>(properties);
            MainCanvas.Children.Clear();
            fp = new FloorPlan(fpWidth, fpHeight, fpMaxRooms, fpMaxRoomsPerGroup, fpMaxRoomSize, fpMaxLargeRooms, fpBlueprintSections, fpfootprintlength, fpfootprintwidth, doPause);
            try
            {
                mainThread = new Thread(fp.GenerateFloorPlan);
                mainThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DrawData(32);
            DrawHallways(32, 8);
            DrawDoors(32, 8);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            fp.RegroupSpanningGroups();
            labelCountRoom.Content = fp.spanningRoomCount.ToString();
            DrawData(32);
            DrawHallways(32, 8);
            DrawDoors(32, 8);
        }

        private void AddDoors_click(object sender, RoutedEventArgs e)
        {
            fp.AssignDoorways();
            DrawData(32);
            DrawHallways(32, 8);
            DrawDoors(32, 8);
        }

        private void ClearDoors_click(object sender, RoutedEventArgs e)
        {
            fp.ClearDoorways();
            DrawData(32);
            DrawHallways(32, 8);
            DrawDoors(32, 8);
        }

        private void DoAll_Click(object sender, RoutedEventArgs e)
        {
            // I have no idea what i'm doing lol.
            fp.RegroupSpanningGroups();
            fp.AssignDoorways();
            DrawData(32);
            DrawHallways(32, 8);
            DrawDoors(32, 8);
            labelCountRoom.Content = "Rooms: " + fp.roomSizeDict.Count.ToString() + " Corridors: " + fp.CorridorGroups.Count.ToString();
        }

        private void SingleDoor_Click(object sender, RoutedEventArgs e)
        {
            fp.AssignDoorway();
            DrawData(32);
            DrawHallways(32, 8);
            DrawDoors(32, 8);
            labelCountRoom.Content = "Rooms: " + fp.roomSizeDict.Count.ToString() + " Corridors: " + fp.CorridorGroups.Count.ToString();
        }

    }
}
