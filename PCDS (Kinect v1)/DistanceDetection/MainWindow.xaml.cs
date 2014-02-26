 using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices; 
using System.Data.Services.Client;
using System.Drawing;
using System.Diagnostics;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Microsoft.Kinect;
using System.Windows.Threading;
namespace TrackingHand
{
    /// <summary>
    /// Patient Contact Detection System
    /// Created by Junyang Chen since May 2013 for CompEpi Group.
    /// All rights reserved by Junyang Chen and CompEpi Group.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Import reacTIVision Library
        /*
         * Integrating Reactivision Fiducial Tracking library, DLL can be found in the debug folder.
         */
        [DllImport("FidTrackerDll2.dll")]
        public static extern unsafe int* getFiducialCount(int h, int w,  byte[] src);
        [DllImport("FidTrackerDll2.dll")]
        public static extern void initializeTreeMap();

        #endregion

        #region Variables
        private DispatcherTimer handTimer = new DispatcherTimer();
        private int TimerTickCountForHand = 0;

        private bool isBoundaryReady = false;

        //The purpose of using general timer is to avoid calling some functions constantly.
        private DispatcherTimer generalTimer = new DispatcherTimer();
        private int TimerTickCount = 0;
        private int CircleLocAvgCount = 0;
        private int[] Circle1DepthAvg = new int[5];
        private int[] Circle2DepthAvg = new int[5];

        /* For storing event data */
        System.IO.StreamWriter file;

        /* Attempt to create a data structure to tie event# to Skeleton ID */
        int[,] eventOfSkeleton = new int[4,2];
        
        /* Box Intializatin:
         * 10 points defining the box*/ 

        private float x_pt1 = 0;
        private float y_pt1 = 0;
        private float z_pt1 = 0;

        private float x_pt2 = 0;
        private float y_pt2 = 0;
        private float z_pt2 = 0;

        private float x_pt3 = 0;
        private float y_pt3 = 0;
        private float z_pt3 = 0;

        private float x_pt4 = 0;
        private float y_pt4 = 0;
        private float z_pt4 = 0;

        private float x_pt5 = 0;
        private float y_pt5 = 0;
        private float z_pt5 = 0;

        private float x_pt6 = 0;
        private float y_pt6 = 0;
        private float z_pt6 = 0;

        private float x_pt7 = 0;
        private float y_pt7 = 0;
        private float z_pt7 = 0;

        private float x_pt8 = 0;
        private float y_pt8 = 0;
        private float z_pt8 = 0;

        private float x_pt9 = 0;
        private float y_pt9 = 0;
        private float z_pt9 = 0;

        private float x_pt10 = 0;
        private float y_pt10 = 0;
        private float z_pt10 = 0;

        private float x_midpt = 0;
        private float z_midpt = 0;
        private float y_midpt = 0;

        private float x_vector_side = 0;
        private float y_vector_side = 0;
        private float z_vector_side = 0;

        private float x_vector_back = 0;
        private float y_vector_back = 0;
        private float z_vector_back = 0;

        Vector4 accel = new Vector4();

        //Times for initialization
        private int leftInit = 0;
        /* End */

        /* Indications for which hand is in the box */
        private Boolean leftHandIn1 = false;
        private Boolean rightHandIn1 = false;
        private Boolean leftHandIn2 = false;
        private Boolean rightHandIn2 = false;

        /* ID storer for hands*/
        private int leftHandCount1 = 0;
        private int rightHandCount1 = 0;
        private int leftHandCount2 = 0;
        private int rightHandCount2 = 0;

        /* Two Hands time counter */
        DateTime rightHandStart1;
        DateTime rightHandEnd1;
        DateTime leftHandStart1;
        DateTime leftHandEnd1;
        DateTime rightHandStart2;
        DateTime rightHandEnd2;
        DateTime leftHandStart2;
        DateTime leftHandEnd2;

        /*Polygon for visualizing the bed*/

        Polygon polygon1 = new Polygon();
        Polygon polygon2 = new Polygon();
        Polygon polygon3 = new Polygon();
        Polygon polygon4 = new Polygon();

        /* Four stopwatchs for for hands*/
        Stopwatch stopwatch = new Stopwatch();

        private Boolean leftSignal = false;

        Skeleton[] totalSkeleton = new Skeleton[6];

        private int timerToggle = Globals.OFF;
        
        public int eventCounter = 0; // Keep track of event #
        
        private Boolean isFileAvail = false;
        private Boolean isBoxUpdating = true;

        /// <value>The pixel data.</value>
        private byte[] grayScaleData;
        private byte[] pixelData;
        private WriteableBitmap writeableBitmap;
        KinectPlacementDialog KinectDialog;
        private KinectSensor sensor;
        private short[] depthTem;
        #endregion

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();
            KinectDialog= new KinectPlacementDialog();
            KinectDialog.ShowDialog();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            stopwatch.Start();

            this.handTimer.Interval = TimeSpan.FromSeconds(1);
            this.handTimer.Tick += new EventHandler(handTimer_Tick);

            this.generalTimer.Interval = TimeSpan.FromSeconds(1);
            this.generalTimer.Tick += generalTimer_Tick;
            generalTimer.Start();
            // Keep generating new data file every time running the program and save it.
            // file = new System.IO.StreamWriter(DateTime.Now.ToString(@"MM-dd-yyyy-h-mm-ss(tt)")+ ".txt");
            String path;
            if( KinectDialog.kinectPlacement == Globals.LEFT_KINECT)
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Data\\data_left.txt"; 
            else
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Data\\data_right.txt"; 

            file = new System.IO.StreamWriter(path);

            if(file != null)
                isFileAvail = true;
            /* Writing header message into the file...*/
            file.WriteLine("============================================================================================");
            file.WriteLine("Box Size: " + Globals.length + " X " +Globals.width  + " X " + Globals.box_height+ " M");
            file.WriteLine("Kinect Position: " + Globals.rx + ", " + Globals.ry + ", " + Globals.rz);
            file.WriteLine("Experiment Time: " + DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm\:ss tt") + " and Ticks:" + DateTime.Now.Ticks);
            file.WriteLine("============================================================================================");
            file.WriteLine("EVENTTYPE | EVENT# | HANDTYPE |     X     |     Y     |     Z     |   TIME   | STOPWATCH(ms) | SKELETON ID");
            // Initialized tree map for fidtracker library
            initializeTreeMap();
            
        }

        void generalTimer_Tick(object sender, EventArgs e)
        {
            TimerTickCount += 1;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            return;
        }
        
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        { 

            // Check if sensor is connected
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Device Connected");
                Application.Current.Shutdown();
                return;
            }

            var connectedSensors = KinectSensor.KinectSensors.Where(sensoritem => sensoritem.Status == KinectStatus.Connected).ToList<KinectSensor>();

            if (connectedSensors.Count == 1)
            {
                this.sensor = connectedSensors[0];
            }
            else if (connectedSensors.Count > 1) // Situation when two Kinects are connected
            {
                if (KinectDialog.kinectPlacement == Globals.LEFT_KINECT)
                    this.sensor = connectedSensors[0];
                else if (KinectDialog.kinectPlacement == Globals.RIGHT_KINECT)
                    this.sensor = connectedSensors[1];
            }
            else
            {
                MessageBox.Show("No Kinect Detected");
                Application.Current.Shutdown();
                return;
            }

            if (!this.sensor.DepthStream.IsEnabled)
            {
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.DepthFrameReady += sensor_DepthFrameReady;
            }

            if (!this.sensor.SkeletonStream.IsEnabled)
            {
                // Enable the skeleton steam with smooth parameters.
                this.sensor.SkeletonStream.Enable();
                this.sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);
            }

            if (!this.sensor.ColorStream.IsEnabled)
            {
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.writeableBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
                    this.sensor.ColorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this.videoControl1.Source = this.writeableBitmap;
                
                this.sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);

                //Preset Camera Setting to help find circles.
                this.sensor.ColorStream.CameraSettings.Contrast = 2.0;
                this.sensor.ColorStream.CameraSettings.Gamma = 2.6;
                this.sensor.ColorStream.CameraSettings.Brightness = 0.3;
                this.sensor.ColorStream.CameraSettings.Saturation = 1.3;
                
            }

            // start the sensor and reset the angle 
            this.sensor.Start();
            try
            {
                this.sensor.ElevationAngle = Globals.KINECT_ANGLE_ZERO;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /* Close the Window */
        private void Window_Closed(object sender, EventArgs e)
        {
            file.WriteLine( "TOTAL EVENTS: " + eventCounter);
            isFileAvail = false;
            file.Flush();
            file.Close();
        }

        #endregion

        #region UI Control
        /* Reset the Kinect Angle to 0 */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.ElevationAngle = Globals.KINECT_ANGLE_ZERO;
        }

        /* Turn off the color image */
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.videoControl1.Visibility = Visibility.Collapsed;
        }

        /* Turn on the color image */
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.videoControl1.Visibility = Visibility.Visible;
        }

        private void gamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.sensor != null && this.sensor.ColorStream.IsEnabled)
            {
                this.sensor.ColorStream.CameraSettings.Gamma = e.NewValue;
            }
        }

        private void contrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.sensor != null && this.sensor.ColorStream.IsEnabled)
            {
                this.sensor.ColorStream.CameraSettings.Contrast = e.NewValue;
            }
        }

        private void saturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.sensor != null && this.sensor.ColorStream.IsEnabled)
            {
                this.sensor.ColorStream.CameraSettings.Saturation = e.NewValue;
            }
        }

        private void brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.sensor != null && this.sensor.ColorStream.IsEnabled)
            {
                this.sensor.ColorStream.CameraSettings.Brightness = e.NewValue;
            }
        }

        private void Button_Click_Update_Box(object sender, RoutedEventArgs e)
        {
            if (isBoxUpdating == true)
            {
                this.update_button.Content = "Enable Box Update";
                isBoxUpdating = false;
            }
            else
            {
                this.update_button.Content = "Disable Box Update";
                isBoxUpdating = true;
            }
        }

        #endregion

        #region Depthframe Actions for Kinect.
        private DepthImageFrame depthImageFrame = null;
        private short[] depthRawValues; // depthImageFrame.PixelDataLength
        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {

            using (depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {
                    depthRawValues = new short[depthImageFrame.PixelDataLength];
                    depthImageFrame.CopyPixelDataTo(depthRawValues);
                }
            }
        }

        #endregion

        #region Colorframes actions for Kinect.

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                //Check if the incoming frame is not null
                if (imageFrame == null)
                {
                    return;
                }
                else
                {
                    // Get the pixel data in byte array
                    this.pixelData = new byte[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(this.pixelData);

                    // Make a copy of depthRawValue to avoid data asyncronized.
                    if( this.depthImageFrame != null && this.depthRawValues != null )
                        depthTem = depthRawValues;
                    else
                        depthTem = null;

                    this.grayScaleData = toGrayScale(this.pixelData, imageFrame.Width, imageFrame.Height);

                    // Only search for fiducial markers every two seconds for computational efficiency
                    if (TimerTickCount % Globals.BOX_UPDATE_EVERY_SECONDS == 0 && TimerTickCount != 0 && isBoxUpdating)
                    {
                        TimerTickCount = 0;
                        unsafe
                        {
                            int* markerLocs = getFiducialCount(imageFrame.Height,imageFrame.Width, grayScaleData); 

                            /* {markerLocs[even]} are the x coordinates of the markers,
                             * {markerLocs[odd]} are the y coordinates of the markers.
                             * Here I am getting only two fiducials markers.
                             */

                                if ( depthTem != null && markerLocs[0] > 0 && markerLocs[2] > 0)
                                {
                                    this.LostCircles.Visibility = System.Windows.Visibility.Hidden;
                                    DepthImagePoint marker1 = new DepthImagePoint();
                                    DepthImagePoint marker2 = new DepthImagePoint();
                                    if (markerLocs[2] > markerLocs[0])
                                    {
                                        marker1.X = markerLocs[0];
                                        marker1.Y = markerLocs[1];
                                        marker2.X = markerLocs[2];
                                        marker2.Y = markerLocs[3];
                                    }
                                    else
                                    {
                                        marker2.X = markerLocs[0];
                                        marker2.Y = markerLocs[1];
                                        marker1.X = markerLocs[2];
                                        marker1.Y = markerLocs[3];
                                    }

                                    marker1.Depth = getAverageDepth(marker1.X, marker1.Y, depthTem);
                                    marker2.Depth = getAverageDepth(marker2.X, marker2.Y, depthTem);
                                    Console.Write("Marker 1: X : " + marker1.X + ", Y: " + marker1.Y + ", Depth: " + marker1.Depth + "; ");
                                    Console.WriteLine("Marker 2: X : " + marker2.X + ", Y: " + marker2.Y + ", Depth: " + marker2.Depth);
                                    
                                    //Make sure the locations are valid
                                    if (marker1.Depth > Globals.LOWER_DEPTH_THRESHOLD && marker1.Depth < Globals.UPPER_DEPTH_THRESHOLD
                                        && marker2.Depth > Globals.LOWER_DEPTH_THRESHOLD && marker2.Depth < Globals.UPPER_DEPTH_THRESHOLD)
                                    {
                                        visualizeCircles(marker1, marker2);
                                        
                                        Circle1DepthAvg[CircleLocAvgCount % 5] = marker1.Depth;
                                        Circle2DepthAvg[CircleLocAvgCount % 5] = marker2.Depth;
                                        CircleLocAvgCount++;

                                        if (CircleLocAvgCount > 5 )
                                        {
                                            int d1 = 0;
                                            int d2 = 0;
                                            int index1 = 0;
                                            int index2 = 0;
                                            Array.Sort(Circle1DepthAvg);
                                            Array.Sort(Circle1DepthAvg);
                                            for (int i = 0; i < 5; i++)
                                            {
                                                // Compare every elements in the array with the median to only calculate average of 
                                                // reasonable depth values.
                                                if (Math.Abs(Circle1DepthAvg[i] - Circle1DepthAvg[2]) < Globals.DIFFERENCE_THRESHOLD)
                                                {
                                                    d1 += Circle1DepthAvg[i];
                                                    index1++;
                                                }
                                                if (Math.Abs(Circle1DepthAvg[i] - Circle1DepthAvg[2]) < Globals.DIFFERENCE_THRESHOLD)
                                                {
                                                    d2 += Circle2DepthAvg[i];
                                                    index2++;
                                                }
                                            }
                                            marker1.Depth = (d1 / index1);
                                            marker2.Depth = (d2 / index2);

                                            create_boundary(marker1, marker2);
                                            TimerTickCount = 0;
                                        }
                                    }
                                }
                        }
                    }
                    else if (TimerTickCount > 15 && isBoxUpdating)
                    {
                        this.LostCircles.Visibility = System.Windows.Visibility.Visible;
                    }
                    
                    // Calcualte the stride
                    int stride = imageFrame.Width * imageFrame.BytesPerPixel;
                    
                    // Assign the bitmap source into image control
                    this.writeableBitmap.WritePixels(new Int32Rect(0, 0, this.writeableBitmap.PixelWidth, this.writeableBitmap.PixelHeight), this.pixelData, stride, 0);

                    
                }
            }
         }

        private int getMedianDepth(int coordinate_x, int coordinate_y, short[] depthTem)
        {
            int[] depth = new int[100];
            int middle_depth;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {

                    middle_depth = depthTem[((coordinate_y - 5) + i) * depthImageFrame.Width + ((coordinate_x - 5) + j)];     
                    depth[i * 10 + j] = middle_depth;
                }
            }
            Array.Sort(depth);
            return depth[50] >> DepthImageFrame.PlayerIndexBitmaskWidth;
        }

        /*we are using this one*/
        private int getAverageDepth(int coordinate_x, int coordinate_y, short[] depthTem)
        {
            int[] depth = new int[100];
            int middle_depth;
            int index = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {

                    middle_depth = depthTem[((coordinate_y - 5) + i) * depthImageFrame.Width + ((coordinate_x - 5) + j)];
                    middle_depth = middle_depth >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    if (middle_depth > Globals.LOWER_DEPTH_THRESHOLD && middle_depth < Globals.UPPER_DEPTH_THRESHOLD)
                    {
                        depth[index] = middle_depth;
                        index++;
                    }
                }
            }
            if (index == 0) return -1;
            int sum = 0;
            for (int i = 0; i < index; i++)
                sum += depth[i];
            return sum / index;
        }

        #endregion

        #region Skeleton Frame Actions for the Kinect.
        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null || !isFileAvail)
                {
                    return;
                }

                skeletonFrame.CopySkeletonDataTo(totalSkeleton);
                //Skeleton Selection Control
                this.skeletonSelection();

                this.displaySkeleton();

                this.logSkeletonData();

                Skeleton firstSkeleton = (from trackskeleton in totalSkeleton
                                          where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                          select trackskeleton).FirstOrDefault();

                Skeleton secondSkeleton = (from trackskeleton in totalSkeleton
                                          where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                          select trackskeleton).LastOrDefault();
               
                if (firstSkeleton == null)
                {
                    turnOffSignal();
                    return;

                }

                else if (firstSkeleton == secondSkeleton)
                {
                    if (firstSkeleton.Joints[JointType.HandRight].TrackingState != JointTrackingState.NotTracked 
                        || firstSkeleton.Joints[JointType.HandLeft].TrackingState != JointTrackingState.NotTracked
                        )
                    {
                        if (isOnOppositeSide(firstSkeleton.Position))
                        {
                            signal1.Fill = new SolidColorBrush(Colors.Green);
                            this.leftSignal = true;
                            this.MapJointsWithUIElement(firstSkeleton.Joints[JointType.HandRight], firstSkeleton.Joints[JointType.HandLeft]);
                            this.handDetected(firstSkeleton);
                        }
                        else {
                            this.stopTimerHand();
                            this.turnOffSignal();
                        }
                    }
                }
                    
                else {
                    if (firstSkeleton.Joints[JointType.HandRight].TrackingState != JointTrackingState.NotTracked
                        || firstSkeleton.Joints[JointType.HandLeft].TrackingState != JointTrackingState.NotTracked
                        || secondSkeleton.Joints[JointType.HandLeft].TrackingState != JointTrackingState.NotTracked
                        || secondSkeleton.Joints[JointType.HandRight].TrackingState != JointTrackingState.NotTracked)
                    {
                        if (isOnOppositeSide(firstSkeleton.Position)
                            && isOnOppositeSide(secondSkeleton.Position))
                        {
                            
                            signal1.Fill = new SolidColorBrush(Colors.Green);
                            this.leftSignal = true;
                            this.MapJointsWithUIElement(
                                firstSkeleton.Joints[JointType.HandLeft], firstSkeleton.Joints[JointType.HandRight],
                                secondSkeleton.Joints[JointType.HandLeft], secondSkeleton.Joints[JointType.HandRight]);
                            this.handDetected(firstSkeleton, secondSkeleton);
                        }
                        else if (isOnOppositeSide(firstSkeleton.Position))
                        {   //Detect only one skeleton
                            Canvas.SetLeft(righthand2, 0);
                            Canvas.SetTop(righthand2, 0);
                            Canvas.SetLeft(lefthand2, 0);
                            Canvas.SetTop(lefthand2, 0);
                            signal1.Fill = new SolidColorBrush(Colors.Green);
                            this.leftSignal = true;

                            this.MapJointsWithUIElement(firstSkeleton.Joints[JointType.HandRight], firstSkeleton.Joints[JointType.HandLeft]);
                            this.handDetected(firstSkeleton);
                           
                        }
                        else if (isOnOppositeSide(secondSkeleton.Position))
                        {
                            Canvas.SetLeft(righthand2, 0);
                            Canvas.SetTop(righthand2, 0);
                            Canvas.SetLeft(lefthand2, 0);
                            Canvas.SetTop(lefthand2, 0);
                            signal1.Fill = new SolidColorBrush(Colors.Green);
                            this.leftSignal = true;

                            this.MapJointsWithUIElement(secondSkeleton.Joints[JointType.HandRight], secondSkeleton.Joints[JointType.HandLeft]);
                            this.handDetected(secondSkeleton);
                        }
                        else {
                            this.stopTimerHand();
                            this.turnOffSignal();
                        }
                    }
                }
             }
        }

        private void displaySkeleton()
        {
            //NEW: count skeletons
            int jointCount = 0;
            int positionCount = 0;
            foreach (Skeleton skeleton in totalSkeleton)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    jointCount += 1;
                    mapTracked(jointCount, skeleton);
                }
                else if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    positionCount += 1;
                    mapPosition(positionCount, skeleton);
                }

                updateLabel(jointCount, positionCount);
            }

            this.joints.Content = jointCount;
            this.positions.Content = positionCount;
        }

        private void logSkeletonData()
        {
            int trackedOrder = 1;
            double newx;
            double newz;
            double newy;
            foreach (Skeleton skeleton in totalSkeleton)
            {
                // Transform the position of skeleton into our uniform coordinate system.
                newx = (x_vector_back * skeleton.Position.X + y_vector_back * skeleton.Position.Y + z_vector_back * skeleton.Position.Z - (x_vector_back * x_pt3 + y_vector_back * y_pt3 + z_vector_back * z_pt3))
                        / Math.Sqrt(x_vector_back * x_vector_back + y_vector_back * y_vector_back + z_vector_back * z_vector_back);


                newz = (x_vector_side * skeleton.Position.X + y_vector_side * skeleton.Position.Y + z_vector_side * skeleton.Position.Z - (x_vector_side * x_pt3 + y_vector_side * y_pt3 + z_vector_side * z_pt3))
                             / Math.Sqrt(x_vector_side * x_vector_side + y_vector_side * y_vector_side + z_vector_side * z_vector_side);

                newy = -(accel.X * skeleton.Position.X + accel.Y * skeleton.Position.Y + accel.Z * skeleton.Position.Z - (accel.X * x_pt3 + accel.Y * y_pt3 + accel.Z * z_pt3))
                             / Math.Sqrt(accel.X * accel.X + accel.Y * accel.Y + accel.Z * accel.Z);

                if (skeleton.TrackingState == SkeletonTrackingState.Tracked )
                {
                   
                    if (trackedOrder == 1)
                    {
                        if (this.leftHandIn1 == true || this.rightHandIn1 == true)
                            file.WriteLine("SKT   | HAND_IN | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + skeleton.TrackingId);
                        else
                            file.WriteLine("SKT   | HAND_OUT | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + skeleton.TrackingId);
                        trackedOrder++;
                    }
                    else if(this.leftHandIn2 == true || this.rightHandIn2 == true)
                        file.WriteLine("SKT   | HAND_IN | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + skeleton.TrackingId);
                    else
                        file.WriteLine("SKT   | HAND_OUT | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + skeleton.TrackingId);
                }
                else if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    file.WriteLine("SKT   | HAND_OUT | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + skeleton.TrackingId);

            }
        }

        
        #endregion /* End */

        #region Hand Detection
        private void handDetected(Skeleton skeleton)
        {
            if ( isBoundaryReady ) // means finished initializing the box
            {
                Joint leftHand1 = skeleton.Joints[JointType.HandLeft];
                Joint rightHand1 = skeleton.Joints[JointType.HandRight];

                /* Is Left Hand tracked? */
                if (leftHand1.TrackingState == JointTrackingState.Tracked)
                {
                    if (isInBox(leftHand1.Position))
                        recordHandIn(leftHand1.Position, Globals.LH1, skeleton.TrackingId);
                    else
                        recordHandOut(leftHand1.Position, Globals.LH1, skeleton.TrackingId);
                }

                if (rightHand1.TrackingState == JointTrackingState.Tracked)
                {
                    if (isInBox(rightHand1.Position))
                        recordHandIn(rightHand1.Position, Globals.RH1, skeleton.TrackingId);
                    else
                        recordHandOut(rightHand1.Position, Globals.RH1, skeleton.TrackingId);
                }

                if (leftHandIn1 || rightHandIn1 )
                {
                    if (this.timerToggle == Globals.OFF)
                        this.startTimerHand();
                }
                else
                {
                    if (this.timerToggle == Globals.ON)
                    {
                        this.stopTimerHand();
                    }
                }
            }
        }
        /* senario for recording hands data for two skeleton, code is lusy */
        private void handDetected(Skeleton skeleton1, Skeleton skeleton2)
        {
            
            if (isBoundaryReady) // means finished initializing the box
            {
                Joint leftHand1 = skeleton1.Joints[JointType.HandLeft];
                Joint rightHand1 = skeleton1.Joints[JointType.HandRight];
                Joint leftHand2 = skeleton2.Joints[JointType.HandLeft];
                Joint rightHand2 = skeleton2.Joints[JointType.HandRight];
                /* Retrieving Hand Data */

                if (leftHand1.TrackingState == JointTrackingState.Tracked)
                {
                    if (isInBox(leftHand1.Position))
                        recordHandIn(leftHand1.Position, Globals.LH1, skeleton1.TrackingId);
                    else
                        recordHandOut(leftHand1.Position, Globals.LH1, skeleton1.TrackingId);
                }

                if (rightHand1.TrackingState == JointTrackingState.Tracked)
                {
                    if (isInBox(rightHand1.Position))
                        recordHandIn(rightHand1.Position, Globals.RH1, skeleton1.TrackingId);
                    else
                        recordHandOut(rightHand1.Position, Globals.RH1, skeleton1.TrackingId);
                }

                if (leftHand2.TrackingState == JointTrackingState.Tracked)
                {
                    if (isInBox(leftHand2.Position))
                        recordHandIn(leftHand2.Position, Globals.LH2, skeleton2.TrackingId);
                    else
                        recordHandOut(leftHand2.Position, Globals.LH2, skeleton2.TrackingId);
                }

                if (rightHand2.TrackingState == JointTrackingState.Tracked)
                {
                    if (isInBox(rightHand2.Position))
                        recordHandIn(rightHand2.Position, Globals.RH2, skeleton2.TrackingId);
                    else
                        recordHandOut(rightHand2.Position, Globals.RH2, skeleton2.TrackingId);
                }

                /* End */
                if (leftHandIn1 || leftHandIn2 || rightHandIn1 || rightHandIn2)
                {
                    if (this.timerToggle == Globals.OFF)
                        this.startTimerHand();
                }
                else
                {
                    if (this.timerToggle == Globals.ON)
                    {
                        this.stopTimerHand();
                    }
                }

            }
        }


        private void recordHandIn(SkeletonPoint joint, int jointType, int skeletonID)
        {
            /*new x, y z are the coordinates related to the bed, the left foot point is the origin in this coordinate system.*/
            double newx = (x_vector_back * joint.X + y_vector_back * joint.Y + z_vector_back * joint.Z - (x_vector_back * x_pt3 + y_vector_back * y_pt3 + z_vector_back * z_pt3)) 
                         / Math.Sqrt(x_vector_back * x_vector_back + y_vector_back * y_vector_back + z_vector_back * z_vector_back);


            double newz = (x_vector_side * joint.X + y_vector_side * joint.Y + z_vector_side * joint.Z - (x_vector_side * x_pt3 + y_vector_side * y_pt3 + z_vector_side * z_pt3))
                         / Math.Sqrt(x_vector_side * x_vector_side + y_vector_side * y_vector_side + z_vector_side * z_vector_side);

            double newy = -(accel.X * joint.X + accel.Y * joint.Y + accel.Z * joint.Z - (accel.X * x_pt3 + accel.Y * y_pt3 + accel.Z * z_pt3))
                         / Math.Sqrt(accel.X * accel.X + accel.Y * accel.Y + accel.Z * accel.Z);

            switch (jointType)
            {
                case Globals.LH1:
                    {
                        if (leftHandIn1 == false)
                        {
                            
                            leftHandCount1 = eventCounter++;
                            leftHandStart1 = DateTime.Now;
                            //associate event number with skeleton ID to avoid data analysis error
                            eventOfSkeleton[Globals.LH1, Globals.EVENT_NUMBER_INDEX] = leftHandCount1;
                            eventOfSkeleton[Globals.LH1, Globals.SKELETON_ID_INDEX] = skeletonID;

                            file.WriteLine("ENTER | " + eventOfSkeleton[Globals.LH1, Globals.EVENT_NUMBER_INDEX] + " | LH1 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.LH1, Globals.SKELETON_ID_INDEX]);
                            leftHandIn1 = true;
                        }
                        else
                        {
                            file.WriteLine("IN    | " + eventOfSkeleton[Globals.LH1, Globals.EVENT_NUMBER_INDEX] + " | LH1 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.LH1, Globals.SKELETON_ID_INDEX]);
                        }
                    }
                    break;
                case Globals.RH1:
                    {

                        if (rightHandIn1 == false)
                        {
                            rightHandCount1 = eventCounter++;
                            rightHandStart1 = DateTime.Now;
                            //associate event number with skeleton ID to avoid data analysis error
                            eventOfSkeleton[Globals.RH1, Globals.EVENT_NUMBER_INDEX] = rightHandCount1;
                            eventOfSkeleton[Globals.RH1, Globals.SKELETON_ID_INDEX] = skeletonID;

                            file.WriteLine("ENTER | " + eventOfSkeleton[Globals.RH1, Globals.EVENT_NUMBER_INDEX] + " | RH1 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.RH1, Globals.SKELETON_ID_INDEX]);
                            rightHandIn1 = true;
                        }
                        else
                        {
                            file.WriteLine("IN    | " + eventOfSkeleton[Globals.RH1, Globals.EVENT_NUMBER_INDEX] + " | RH1 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.RH1, Globals.SKELETON_ID_INDEX]);
                        }
                    }
                    break;
                case Globals.LH2:
                    {
                        if (leftHandIn2 == false)
                        {
                            leftHandCount2 = eventCounter++;
                            leftHandStart2 = DateTime.Now;
                            //associate event number with skeleton ID to avoid data analysis error
                            eventOfSkeleton[Globals.LH2, Globals.EVENT_NUMBER_INDEX] = leftHandCount2;
                            eventOfSkeleton[Globals.LH2, Globals.SKELETON_ID_INDEX] = skeletonID;

                            file.WriteLine("ENTER | " + eventOfSkeleton[Globals.LH2, Globals.EVENT_NUMBER_INDEX] + " | LH2 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.LH2, Globals.SKELETON_ID_INDEX]);
                            leftHandIn2 = true;
                        }
                        else
                        {
                            file.WriteLine("IN    | " + eventOfSkeleton[Globals.LH2, Globals.EVENT_NUMBER_INDEX] + " | LH2 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.LH2, Globals.SKELETON_ID_INDEX]);
                        }
                    }
                    break;
                case Globals.RH2:
                    {
                        if (rightHandIn2 == false)
                        {
                            rightHandCount2 = eventCounter++;
                            rightHandStart2 = DateTime.Now;
                            //associate event number with skeleton ID to avoid data analysis error
                            eventOfSkeleton[Globals.RH2, Globals.EVENT_NUMBER_INDEX] = rightHandCount2;
                            eventOfSkeleton[Globals.RH2, Globals.SKELETON_ID_INDEX] = skeletonID;

                            file.WriteLine("ENTER | " + eventOfSkeleton[Globals.RH2, Globals.EVENT_NUMBER_INDEX] + " | RH2 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.RH2, Globals.SKELETON_ID_INDEX]);
                            rightHandIn2 = true;
                        }
                        else
                        {
                            file.WriteLine("IN    | " + eventOfSkeleton[Globals.RH2, Globals.EVENT_NUMBER_INDEX] + " | RH2 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.RH2, Globals.SKELETON_ID_INDEX]);
                        }
                    }
                    break;
            }
        }

        private void recordHandOut(SkeletonPoint joint, int jointType, int skeletonID)
        {
            double newx = Math.Abs(x_vector_back * joint.X + y_vector_back * joint.Y + z_vector_back * joint.Z - (x_vector_back * x_pt3 + y_vector_back * y_pt3 + z_vector_back * z_pt3))
                         / Math.Sqrt(x_vector_back * x_vector_back + y_vector_back * y_vector_back + z_vector_back * z_vector_back);

            double newz = Math.Abs(x_vector_side * joint.X + y_vector_side * joint.Y + z_vector_side * joint.Z - (x_vector_side * x_pt3 + y_vector_side * y_pt3 + z_vector_side * z_pt3))
                         / Math.Sqrt(x_vector_side * x_vector_side + y_vector_side * y_vector_side + z_vector_side * z_vector_side);

            double newy = Math.Abs(accel.X * joint.X + accel.Y * joint.Y + accel.Z * joint.Z - (accel.X * x_pt3 + accel.Y * y_pt3 + accel.Z * z_pt3))
                         / Math.Sqrt(accel.X * accel.X + accel.Y * accel.Y + accel.Z * accel.Z);

            switch (jointType)
            {
                case Globals.LH1:
                    {
                        if (leftHandIn1)
                        {
                            leftHandEnd1 = DateTime.Now;
                            file.WriteLine("EXIT  | " + eventOfSkeleton[Globals.LH1, Globals.EVENT_NUMBER_INDEX] + " | LH1 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.LH1, Globals.SKELETON_ID_INDEX]);
                            leftHandIn1 = false;
                        }
                    }
                    break;
                case Globals.RH1:
                    {
                        if (rightHandIn1)
                        {
                            rightHandEnd1 = DateTime.Now;
                            file.WriteLine("EXIT  | " + eventOfSkeleton[Globals.RH1, Globals.EVENT_NUMBER_INDEX] + " | RH1 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.RH1, Globals.SKELETON_ID_INDEX]);
                            rightHandIn1 = false;
                        }
                    }
                    break;
                case Globals.LH2:
                    {
                        if (leftHandIn2)
                        {
                            leftHandEnd2 = DateTime.Now;
                            file.WriteLine("EXIT  | " + eventOfSkeleton[Globals.LH2, Globals.EVENT_NUMBER_INDEX] + " | LH2 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.LH2, Globals.SKELETON_ID_INDEX]);
                            leftHandIn2 = false;
                        }
                    }
                    break;
                case Globals.RH2:
                    {
                        if (rightHandIn2)
                        {
                            rightHandEnd2 = DateTime.Now;
                            file.WriteLine("EXIT  | " + eventOfSkeleton[Globals.RH2, Globals.EVENT_NUMBER_INDEX] + " | RH2 | " + newx.ToString("f7") + " | " + newy.ToString("f7") + " | " + newz.ToString("f7") + " | " + DateTime.Now.ToString(@"h\:mm\:sstt") + " | " + stopwatch.ElapsedMilliseconds + " | " + eventOfSkeleton[Globals.RH2, Globals.SKELETON_ID_INDEX]);
                            rightHandIn2 = false;
                        }
                    }
                    break;
            }
        }

        private Boolean isOnOppositeSide(SkeletonPoint position)
        {
            if (KinectDialog.kinectPlacement == Globals.LEFT_KINECT)
            {
                if (position.X > -(y_vector_back * (position.Y - y_midpt) + z_vector_back * (position.Z - z_midpt)) / x_vector_back + x_midpt)
                    return true;
            }
            else if (KinectDialog.kinectPlacement == Globals.RIGHT_KINECT)
            {
                if (position.X < -(y_vector_back * (position.Y - y_midpt) + z_vector_back * (position.Z - z_midpt)) / x_vector_back + x_midpt)
                    return true;
            }
            return false;
        }

        #endregion

        #region TimerManagement
        private void startTimerHand()
        {
            this.handTimer.Start();
            this.timerToggle = Globals.ON;
        }

        private void stopTimerHand()
        {
            this.timeProgressBar.Value = 0;
            this.handTimer.Stop();
            this.TimerTickCountForHand = 0;
            this.timerToggle = Globals.OFF;
        }

        public void handTimer_Tick(object sender, object e)
        {
            this.TimerTickCountForHand++;
            this.timeProgressBar.Value = this.TimerTickCountForHand * 5; //for the total of 10s
        }

        #endregion

        #region Box Generation
        private void create_boundary(DepthImagePoint marker1, DepthImagePoint marker2)
        {
            this.generalTimer.Start();
            isBoundaryReady = true;
            /* Keep reading accelerometer value until it is valid */
            int reading = 0;
            do 
            {
                accel = sensor.AccelerometerGetCurrentReading();
                reading++;
            } while (accel.X == 0 || accel.X == -1);
            /* If it still failed after trying 10 times, then exit. */
            if (reading > 10)
            {
                return;
            }
            
            SkeletonPoint point1 = this.sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, marker1);
            SkeletonPoint point2 = this.sensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, marker2);

            //middle point of the circles 
            x_midpt = (point1.X + point2.X) / 2;
            z_midpt = (point1.Z + point2.Z) / 2;
            y_midpt = (point1.Y + point2.Y) / 2; 

            //vector of the line of the circles
            x_vector_back = point2.X - point1.X;
            y_vector_back = point2.Y - point1.Y;
            z_vector_back = point2.Z - point1.Z;

            double len_vector_back = Math.Sqrt(x_vector_back * x_vector_back + y_vector_back * y_vector_back + z_vector_back * z_vector_back);
            
            x_vector_back =(float)( x_vector_back / len_vector_back);
            y_vector_back = (float)(y_vector_back / len_vector_back);
            z_vector_back = (float)(z_vector_back / len_vector_back);

            // Box Point 1: left foot point
            double amt = (Globals.width /2 );

            x_pt1 = (float)(x_midpt - x_vector_back * amt);
            y_pt1 = (float)(y_midpt - y_vector_back * amt);
            z_pt1 = (float)(z_midpt - z_vector_back * amt);
            // Box Point 2: right foot point
            x_pt2 = (float)(x_midpt + x_vector_back * amt);
            y_pt2 = (float)(y_midpt + y_vector_back * amt);
            z_pt2 = (float)(z_midpt + z_vector_back * amt);

            //Console.WriteLine(x_pt1);


            //Start calculate all the other eight points for the bed.
            //double unit = (Globals.circle_height) / Math.Sqrt(accel.X * accel.X + accel.Y * accel.Y + accel.Z * accel.Z);
            //point 3
            x_pt3 = (float)(x_pt1 + accel.X * Globals.circle_height);
            y_pt3 = (float)(y_pt1 + accel.Y * Globals.circle_height);
            z_pt3 = (float)(z_pt1 + accel.Z * Globals.circle_height);

            //point 4
            x_pt4 = (float)(x_pt2 + accel.X * Globals.circle_height);
            y_pt4 = (float)(y_pt2 + accel.Y * Globals.circle_height);
            z_pt4 = (float)(z_pt2 + accel.Z * Globals.circle_height);

            //unit = (Globals.box_height) / Math.Sqrt(accel.X * accel.X + accel.Y * accel.Y + accel.Z * accel.Z);
            //point 5
            x_pt5 = (float)(x_pt3 - accel.X * Globals.box_height);
            y_pt5 = (float)(y_pt3 - accel.Y * Globals.box_height);
            z_pt5 = (float)(z_pt3 - accel.Z * Globals.box_height);

            //point 6
            x_pt6 = (float)(x_pt4 - accel.X * Globals.box_height);
            y_pt6 = (float)(y_pt4 - accel.Y * Globals.box_height);
            z_pt6 = (float)(z_pt4 - accel.Z * Globals.box_height );

            /*
             * Now calculate the vector that is parellel to the the sides of the bed, pointing toward the head.
             * vector a is the vector from point 3 -> point 5, vector b is the vector from point 3 -> point 4.
             * x_a, y_a, z_a stand for the three coordinates, same for the other cases.
             */
            float x_a = x_pt5 - x_pt3;
            float y_a = y_pt5 - y_pt3;
            float z_a = z_pt5 - z_pt3;

            float x_b = x_pt4 - x_pt3;
            float y_b = y_pt4 - y_pt3;
            float z_b = z_pt4 - z_pt3;

            x_vector_side = y_a * z_b - z_a * y_b;
            y_vector_side = z_a * x_b - x_a * z_b;
            z_vector_side = x_a * y_b - y_a * x_b;

            double len_vector_side = Math.Sqrt(x_vector_side * x_vector_side + y_vector_side * y_vector_side + z_vector_side * z_vector_side);
            x_vector_side = (float) (x_vector_side / len_vector_side);
            y_vector_side = (float)(y_vector_side / len_vector_side);
            z_vector_side = (float)(z_vector_side / len_vector_side);

            //point 7
            x_pt7 = (float)(x_pt3 + x_vector_side * Globals.length);
            y_pt7 = (float)(y_pt3 + y_vector_side * Globals.length);
            z_pt7 = (float)(z_pt3 + z_vector_side * Globals.length);

            //point 8
            x_pt8 = (float)(x_pt4 + x_vector_side * Globals.length);
            y_pt8 = (float)(y_pt4 + y_vector_side * Globals.length);
            z_pt8 = (float)(z_pt4 + z_vector_side * Globals.length);

            //point 9
            x_pt9 = (float)(x_pt5 + x_vector_side * Globals.length);
            y_pt9 = (float)(y_pt5 + y_vector_side * Globals.length);
            z_pt9 = (float)(z_pt5 + z_vector_side * Globals.length);

            //point 10
            x_pt10 = (float)(x_pt6 + x_vector_side * Globals.length);
            y_pt10 = (float)(y_pt6 + y_vector_side * Globals.length);
            z_pt10 = (float)(z_pt6 + z_vector_side * Globals.length);


            //Visualize the bed
            SkeletonPoint drawpoint1 = new SkeletonPoint();
            drawpoint1.X = x_pt1;
            drawpoint1.Y = y_pt1;
            drawpoint1.Z = z_pt1;

            SkeletonPoint drawpoint2 = new SkeletonPoint();
            drawpoint2.X = x_pt2;
            drawpoint2.Y = y_pt2;
            drawpoint2.Z = z_pt2;

            SkeletonPoint drawpoint3 = new SkeletonPoint();
            drawpoint3.X = x_pt3;
            drawpoint3.Y = y_pt3;
            drawpoint3.Z = z_pt3;

            SkeletonPoint drawpoint4 = new SkeletonPoint();
            drawpoint4.X = x_pt4;
            drawpoint4.Y = y_pt4;
            drawpoint4.Z = z_pt4;

            SkeletonPoint drawpoint5 = new SkeletonPoint();
            drawpoint5.X = x_pt5;
            drawpoint5.Y = y_pt5;
            drawpoint5.Z = z_pt5;

            SkeletonPoint drawpoint6 = new SkeletonPoint();
            drawpoint6.X = x_pt6;
            drawpoint6.Y = y_pt6;
            drawpoint6.Z = z_pt6;

            SkeletonPoint drawpoint7 = new SkeletonPoint();
            drawpoint7.X = x_pt7;
            drawpoint7.Y = y_pt7;
            drawpoint7.Z = z_pt7;

            SkeletonPoint drawpoint8 = new SkeletonPoint();
            drawpoint8.X = x_pt8;
            drawpoint8.Y = y_pt8;
            drawpoint8.Z = z_pt8;

            SkeletonPoint drawpoint9 = new SkeletonPoint();
            drawpoint9.X = x_pt9;
            drawpoint9.Y = y_pt9;
            drawpoint9.Z = z_pt9;

            SkeletonPoint drawpoint10 = new SkeletonPoint();
            drawpoint10.X = x_pt10;
            drawpoint10.Y = y_pt10;
            drawpoint10.Z = z_pt10;

            bedVisualization(drawpoint1, drawpoint2, drawpoint3, drawpoint4, drawpoint5, drawpoint6, drawpoint7, drawpoint8, drawpoint9, drawpoint10);
            
            this.leftInit += 1;
            this.leftBox.Text += ("Kinect Initialized for " + leftInit + " times.\n");
            this.leftBox.ScrollToEnd();   
        }

        /* Decides whether a Skeletal Point is in Box */
        private Boolean isInBox(SkeletonPoint sp)
        {
            if (sp.X > -(y_vector_back * (sp.Y - y_pt3) + z_vector_back * (sp.Z - z_pt3)) / x_vector_back + x_pt3
               && sp.X < -(y_vector_back * (sp.Y - y_pt4) + z_vector_back * (sp.Z - z_pt4)) / x_vector_back + x_pt4
               && sp.Z < -(x_vector_side * (sp.X - x_pt3) + y_vector_side * (sp.Y - y_pt3)) / z_vector_side + z_pt3
               && sp.Z > -(x_vector_side * (sp.X - x_pt7) + y_vector_side * (sp.Y - y_pt7)) / z_vector_side + z_pt7)
                return true;
            else
            {
                return false;   
            }
        }
        #endregion

        #region UI Graphics
        /// <summary>
        /// Maps the joints with UI element.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        private void MapJointsWithUIElement(Joint joint1, Joint joint2)
        {
            // Get the Points in 2D Space to map in UI. for that call the Scale position method.
            DepthImagePoint mappedPoint = this.ScalePosition(joint1.Position);
            Canvas.SetLeft(righthand1, mappedPoint.X);
            Canvas.SetTop(righthand1, mappedPoint.Y);

            DepthImagePoint mappedPoint2 = this.ScalePosition(joint2.Position);
            Canvas.SetLeft(lefthand1, mappedPoint2.X);
            Canvas.SetTop(lefthand1, mappedPoint2.Y);
            Canvas.SetZIndex(lefthand1, 100);
            Canvas.SetZIndex(righthand1, 100);
        }

        private void MapJointsWithUIElement(Joint left1, Joint right1, Joint left2, Joint right2)
        {
            // Get the Points in 2D Space to map in UI. for that call the Scale postion method.
            DepthImagePoint mappedPoint = this.ScalePosition(left1.Position);
            Canvas.SetLeft(righthand1, mappedPoint.X);
            Canvas.SetTop(righthand1, mappedPoint.Y);

            DepthImagePoint mappedPoint2 = this.ScalePosition(right1.Position);
            Canvas.SetLeft(lefthand1, mappedPoint2.X);
            Canvas.SetTop(lefthand1, mappedPoint2.Y);


            DepthImagePoint mappedPoint3 = this.ScalePosition(left2.Position);
            Canvas.SetLeft(lefthand2, mappedPoint3.X);
            Canvas.SetTop(lefthand2, mappedPoint3.Y);

            DepthImagePoint mappedPoint4 = this.ScalePosition(right2.Position);
            Canvas.SetLeft(righthand2, mappedPoint4.X);
            Canvas.SetTop(righthand2, mappedPoint4.Y);

            Canvas.SetZIndex(lefthand1, 100);
            Canvas.SetZIndex(righthand1, 100);
            Canvas.SetZIndex(lefthand2, 100);
            Canvas.SetZIndex(righthand2, 100);
        }

        private void visualizeCircles(DepthImagePoint point1, DepthImagePoint point2)
        {
            Canvas.SetLeft(circle1, point1.X);
            Canvas.SetTop(circle1, point1.Y);

            Canvas.SetLeft(circle2, point2.X);
            Canvas.SetTop(circle2, point2.Y);
        }

        private void mapTracked(int number, Skeleton s)
        {
            DepthImagePoint dp = ScalePosition(s.Position);
            switch (number)
            {
                case 1:
                    {
                        Canvas.SetLeft(Track1, dp.X);
                        Canvas.SetTop(Track1, dp.Y);
                        Track1.Content = s.TrackingId;
                        Track1.Visibility = System.Windows.Visibility.Visible;
                    }
                break;
                case 2:
                    {
                        Canvas.SetLeft(Track2, dp.X);
                        Canvas.SetTop(Track2, dp.Y);
                        Track2.Content = s.TrackingId;
                        Track2.Visibility = System.Windows.Visibility.Visible;
                    }
                break;
            }
        
        }

        private void mapPosition(int number, Skeleton s)
        {
            DepthImagePoint dp = ScalePosition(s.Position);
            switch (number)
            {
                case 1:
                    {
                        Canvas.SetLeft(Position1, dp.X);
                        Canvas.SetTop(Position1, dp.Y);
                        Position1.Content = s.TrackingId;
                        Position1.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case 2:
                    {
                        Canvas.SetLeft(Position2, dp.X);
                        Canvas.SetTop(Position2, dp.Y);
                        Position2.Content = s.TrackingId;
                        Position2.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case 3:
                    {
                        Canvas.SetLeft(Position3, dp.X);
                        Canvas.SetTop(Position3, dp.Y);
                        Position3.Content = s.TrackingId;
                        Position3.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case 4:
                    {
                        Canvas.SetLeft(Position4, dp.X);
                        Canvas.SetTop(Position4, dp.Y);
                        Position4.Content = s.TrackingId;
                        Position4.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
            }
        }

        private void turnOffSignal()
        {
            this.signal1.Fill = new SolidColorBrush(Colors.Red);
            this.position1.Content = "";
            leftSignal = false;

            this.stopTimerHand();

            //Set points to default position
            Canvas.SetLeft(lefthand1, 0);
            Canvas.SetTop(lefthand1, 0);
            Canvas.SetLeft(righthand1, 0);
            Canvas.SetTop(righthand1, 0);

            Canvas.SetLeft(lefthand2, 0);
            Canvas.SetTop(lefthand2, 0);
            Canvas.SetLeft(righthand2, 0);
            Canvas.SetTop(righthand2, 0);
        }

        private void updateLabel(int jointCount, int positionCount)
        {
            switch (jointCount)
            {
                case 0: 
                    {
                        Track1.Visibility = System.Windows.Visibility.Hidden;
                        Track2.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
                case 1:
                    {
                        Track2.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
            }

            switch (positionCount)
            {
                case 0:
                    {
                        Position1.Visibility = System.Windows.Visibility.Hidden;
                        Position2.Visibility = System.Windows.Visibility.Hidden;
                        Position3.Visibility = System.Windows.Visibility.Hidden;
                        Position4.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
                case 1:
                    {
                        Position2.Visibility = System.Windows.Visibility.Hidden;
                        Position3.Visibility = System.Windows.Visibility.Hidden;
                        Position4.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
                case 2:
                    {
                        Position3.Visibility = System.Windows.Visibility.Hidden;
                        Position4.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
                case 3:
                    {
                        Position4.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
            }
        }

        private void bedVisualization(SkeletonPoint drawpoint1, SkeletonPoint drawpoint2, SkeletonPoint drawpoint3, SkeletonPoint drawpoint4,
            SkeletonPoint drawpoint5, SkeletonPoint drawpoint6, SkeletonPoint drawpoint7, SkeletonPoint drawpoint8,
                SkeletonPoint drawpoint9, SkeletonPoint drawpoint10)
        {

            DepthImagePoint dp1 = ScalePosition(drawpoint1);
            DepthImagePoint dp2 = ScalePosition(drawpoint2);
            DepthImagePoint dp3 = ScalePosition(drawpoint3);
            DepthImagePoint dp4 = ScalePosition(drawpoint4);
            DepthImagePoint dp5 = ScalePosition(drawpoint5);
            DepthImagePoint dp6 = ScalePosition(drawpoint6);
            DepthImagePoint dp7 = ScalePosition(drawpoint7);
            DepthImagePoint dp8 = ScalePosition(drawpoint8);
            DepthImagePoint dp9 = ScalePosition(drawpoint9);
            DepthImagePoint dp10 = ScalePosition(drawpoint10);

            System.Windows.Point p1 = new System.Windows.Point(dp1.X, dp1.Y);
            System.Windows.Point p2 = new System.Windows.Point(dp2.X, dp2.Y);
            System.Windows.Point p3 = new System.Windows.Point(dp3.X, dp3.Y);
            System.Windows.Point p4 = new System.Windows.Point(dp4.X, dp4.Y);
            System.Windows.Point p5 = new System.Windows.Point(dp5.X, dp5.Y);
            System.Windows.Point p6 = new System.Windows.Point(dp6.X, dp6.Y);
            System.Windows.Point p7 = new System.Windows.Point(dp7.X, dp7.Y);
            System.Windows.Point p8 = new System.Windows.Point(dp8.X, dp8.Y);
            System.Windows.Point p9 = new System.Windows.Point(dp9.X, dp9.Y);
            System.Windows.Point p10 = new System.Windows.Point(dp10.X, dp10.Y);

            System.Windows.Media.PointCollection pointCollection1 = new System.Windows.Media.PointCollection();
            pointCollection1.Add(p3);
            pointCollection1.Add(p4);
            pointCollection1.Add(p6);
            pointCollection1.Add(p5);

            System.Windows.Media.PointCollection pointCollection2 = new System.Windows.Media.PointCollection();
            pointCollection2.Add(p3);
            pointCollection2.Add(p7);
            pointCollection2.Add(p9);
            pointCollection2.Add(p5);

            System.Windows.Media.PointCollection pointCollection3 = new System.Windows.Media.PointCollection();
            pointCollection3.Add(p6);
            pointCollection3.Add(p4);
            pointCollection3.Add(p8);
            pointCollection3.Add(p10);

            System.Windows.Media.PointCollection pointCollection4 = new System.Windows.Media.PointCollection();
            pointCollection4.Add(p7);
            pointCollection4.Add(p8);
            pointCollection4.Add(p10);
            pointCollection4.Add(p9);

            polygon1.Points = pointCollection1;
            polygon1.Stroke = System.Windows.Media.Brushes.Green;
            polygon1.StrokeThickness = 5;
            polygon1.Opacity = 0.4;

            polygon2.Points = pointCollection2;
            polygon2.Stroke = System.Windows.Media.Brushes.Green;
            polygon2.StrokeThickness = 5;
            polygon2.Opacity = 0.4;

            polygon3.Points = pointCollection3;
            polygon3.Stroke = System.Windows.Media.Brushes.Green;
            polygon3.StrokeThickness = 5;
            polygon3.Opacity = 0.4;

            polygon4.Points = pointCollection4;
            polygon4.Stroke = System.Windows.Media.Brushes.Green;
            polygon4.StrokeThickness = 5;
            polygon4.Opacity = 0.4;

            canvas1.Children.Remove(polygon1);
            canvas1.Children.Remove(polygon2);
            canvas1.Children.Remove(polygon3);
            canvas1.Children.Remove(polygon4);

            canvas1.Children.Add(polygon1);
            canvas1.Children.Add(polygon2);
            canvas1.Children.Add(polygon3);
            canvas1.Children.Add(polygon4);
        }

        #endregion

        #region Skeleton Selection Algorithm
        private void skeletonSelection()
        {
            if (this.sensor != null && this.sensor.SkeletonStream != null)
            {
                if (!this.sensor.SkeletonStream.AppChoosesSkeletons)
                {
                    this.sensor.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
                }

                int[] trackedSkeletonID = new int[2];
                int trackedNumber = 0;

                foreach (Skeleton skeleton in this.totalSkeleton.Where(s => s.TrackingState == SkeletonTrackingState.Tracked))
                {
                    if (inDetectRange(skeleton.Position) && trackedNumber < 2)
                    {
                        trackedSkeletonID[trackedNumber] = skeleton.TrackingId;
                        trackedNumber++;
                    }
                }

                if (trackedNumber == 0)
                {
                    foreach (Skeleton skeleton in this.totalSkeleton.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                    {
                        if (inDetectRange(skeleton.Position) && trackedNumber < 2)
                        {
                            trackedSkeletonID[trackedNumber] = skeleton.TrackingId;
                            trackedNumber++;
                        }
                    }
                }
                else if (trackedNumber == 1)
                {
                    foreach (Skeleton skeleton in this.totalSkeleton.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                    {
                        if (inDetectRange(skeleton.Position) && skeleton.TrackingId != trackedSkeletonID[0])
                        {
                            trackedSkeletonID[1] = skeleton.TrackingId;
                            trackedNumber = 2;
                        }
                    }
                }

                if (trackedNumber == 2) this.sensor.SkeletonStream.ChooseSkeletons(trackedSkeletonID[0], trackedSkeletonID[1]);
                else if (trackedNumber == 1) this.sensor.SkeletonStream.ChooseSkeletons(trackedSkeletonID[0]);
                else this.sensor.SkeletonStream.ChooseSkeletons();
            }
        }

        private Boolean inDetectRange(SkeletonPoint position)
        {
            double xToMidPlane = Math.Abs(x_vector_back * position.X + y_vector_back * position.Y + z_vector_back * position.Z - (x_vector_back * x_midpt + y_vector_back * y_midpt + z_vector_back * z_midpt))
                         / Math.Sqrt(x_vector_back * x_vector_back + y_vector_back * y_vector_back + z_vector_back * z_vector_back);

            //Create a middle point in plane 3
            float x_m = (x_pt4 + x_pt8) / 2;
            float y_m = (y_pt4 + y_pt8) / 2;
            float z_m = (z_pt4 + z_pt8) / 2;
            double zToMidPlane = Math.Abs(x_vector_side * position.X + y_vector_side * position.Y + z_vector_side * position.Z - (x_vector_side * x_m + y_vector_side * y_m + z_vector_side * z_m))
                         / Math.Sqrt(x_vector_side * x_vector_side + y_vector_side * y_vector_side + z_vector_side * z_vector_side);

            if (xToMidPlane < (Globals.width / 2 + Globals.Range) && zToMidPlane < (Globals.length / 2 + Globals.Range))
                return true;
            else
                return false;
        }
        #endregion

        #region Helper Methods
        /* Transforms SkeletonPoint to a DepthImagePoint for visualizing hands */
        private DepthImagePoint ScalePosition(SkeletonPoint skeletonPoint)
        {
            // return the depth points from the skeleton point
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return depthPoint;
        }

        
        /*
         * Generating binary byte array containing only 255 and 0 values from color data array. 
         */ 
        private byte[] toGrayScale(byte[] pixelData, int width, int height)
        {
            byte[] grayCharArray = new byte[width * height];
            int firstIndex;
            int i =0;
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    firstIndex = x * width * Globals.BYTES_PER_PIXEL + y * Globals.BYTES_PER_PIXEL;

                    if((int)pixelData[firstIndex] > 120 && (int)pixelData[firstIndex + 1] > 120 && (int)pixelData[firstIndex + 2] > 120)
                    {
                        grayCharArray[i] = (byte)255;
                    }
                    else
                    {
                        grayCharArray[i] = (byte)0;
                    }
                    i++;
                }
            }
            return grayCharArray;
        }
        #endregion
    }
}
