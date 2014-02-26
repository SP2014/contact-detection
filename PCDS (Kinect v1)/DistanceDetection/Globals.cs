using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
namespace TrackingHand
{
    class Globals
    {
        public const int ON = 1;
        public const int OFF = 0;
        //macros for auto-adjustment
        public const int UP = 1;
        public const int DOWN = 0;

        //size for the medical bed
        //public const float length = 2.0F;
        //public const float width = 0.85F;
        //size for experiment table
        public const float length = 2.25F;
        public const float width = 1.0F;
        
        public const float height = 0.735F;

        //position of the kinects
        public const float rx = 0.6F;
        public const float rz = 0.35F;
        public const float ry = 0.35F;
        //public const float ry = 0.05F;
        //height for the circles
        public const float circle_height = 0.135F;
        //public const float circle_height = 0.235F;

        public const float box_height = 0.8F;


        //emun for joint type
        public const int RH1 = 0;
        public const int LH1 = 1;
        public const int RH2 = 2;
        public const int LH2 = 3;

        //Skeleton Detecting Zone Arround the Bed.
        public const float Range = 1.0F;
 
        public const int KINECT_ANGLE_ZERO = 0;   /* Kinect Default Angle */
        public const int BYTES_PER_PIXEL = 4;
        public const int BOX_UPDATE_EVERY_SECONDS = 2;

        //Left Kinect or Right Kinect (considering standing at the end of the bed and looking at the placement of Kinects)
        public const int LEFT_KINECT = 0;
        public const int RIGHT_KINECT = 1;

        //Threshold for definding the valid depth value
        public const int LOWER_DEPTH_THRESHOLD = 1000;
        public const int UPPER_DEPTH_THRESHOLD = 3000;

        //Macros for associating event number with skeleton ID
        public const int EVENT_NUMBER_INDEX = 0;
        public const int SKELETON_ID_INDEX = 1;

        //threshold for reasonable depth values of fiducial markers
        public const int DIFFERENCE_THRESHOLD = 60;
    }
}
