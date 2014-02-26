/* Gesture Recognition Engine */

namespace GestureRecognizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Kinect;

    /// <summary>
    /// Gesture Recognition Engine
    /// </summary>
    public class GestureRecognitionEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GestureRecognitionEngine" /> class.
        /// </summary>
        public GestureRecognitionEngine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureRecognitionEngine" /> class.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        public GestureRecognitionEngine(Skeleton skeleton)
        {
            this.Skeleton = skeleton;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureRecognitionEngine" /> class.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <param name="gestureType">Type of the gesture.</param>
        public GestureRecognitionEngine(Skeleton skeleton, GestureType gestureType)
        {
            this.Skeleton = skeleton;
            this.GestureType = gestureType;
        }


        /// <summary>
        /// 
        /// </summary>
        float previousDistance = 0.0f;

        /// <summary>
        /// Occurs when [gesture recognized].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        public event EventHandler<GestureEventArgs> GestureNotRecognized;

        public float RecognizeParameter { get; set; }

        /// <summary>
        /// Gets or sets the type of the gesture.
        /// </summary>
        /// <value>
        /// The type of the gesture.
        /// </value>
        public GestureType GestureType { get; set; }

        /// <summary>
        /// Gets or sets the skeleton.
        /// </summary>
        /// <value>
        /// The skeleton.
        /// </value>
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Starts the recognize.
        /// </summary>
        public void StartRecognize()
        {
            switch (this.GestureType)
            {
                case GestureType.HandsClapping:
                    this.MatchHandClappingGesture(this.Skeleton);
                    break;
                case GestureType.HandJointDistance:
                    this.MatchHandDistance(this.Skeleton, GestureType.HandJointDistance);
                    break;
                case GestureType.RightHandAndFeet:
                    this.MatchHandDistance(this.Skeleton, GestureType.RightHandAndFeet);
                    break;
                case GestureType.LeftHandAndFeet:
                    this.MatchHandDistance(this.Skeleton, GestureType.LeftHandAndFeet);
                    break;
                default:
                    break;
            }
        }

        private void MatchHandDistance(Skeleton skeleton, GestureType gestureType)
        {
            float currentDistance = 0.0f;
            if (gestureType == GestureType.HandJointDistance)
            {
                currentDistance = GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.HandLeft]);
            }
            if (gestureType == GestureType.RightHandAndFeet)
            {
                currentDistance = GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.FootRight]);
            }
            if (gestureType == GestureType.LeftHandAndFeet)
            {
                currentDistance = GetJointDistance(skeleton.Joints[JointType.HandLeft], skeleton.Joints[JointType.FootLeft]);
            }
            if (currentDistance > this.RecognizeParameter && previousDistance > 0.1f)
            {
                if (this.GestureRecognized != null)
                {
                    this.GestureRecognized(this, new GestureEventArgs(RecognitionResult.Success));
                }
            }
            else
            {
                if (this.GestureNotRecognized != null)
                {
                    this.GestureNotRecognized(this, new GestureEventArgs(RecognitionResult.Failed));
                }
            }

            previousDistance = currentDistance;
        }

        /// <summary>
        /// Gets the joint distance.
        /// </summary>
        /// <param name="firstJoint">The first joint.</param>
        /// <param name="secondJoint">The second joint.</param>
        /// <returns>retunr the distance</returns>
        private static float GetJointDistance(Joint firstJoint, Joint secondJoint)
        {
            float distanceX = firstJoint.Position.X - secondJoint.Position.X;
            float distanceY = firstJoint.Position.Y - secondJoint.Position.Y;
            float distanceZ = firstJoint.Position.Z - secondJoint.Position.Z;
            return (float)Math.Sqrt(Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2) + Math.Pow(distanceZ, 2));
        }

        /// <summary>
        /// Handles the GestureRecognized event of the GestureRecognitionEngine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GestureEventArgs" /> instance containing the event data.</param>
        private void GestureRecognitionEngine_GestureRecognized(object sender, GestureEventArgs e)
        {
            if (this.GestureRecognized != null)
            {
                this.GestureRecognized(this, e);
            }
        }

        /// <summary>
        /// Matches the hand clapping gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        private void MatchHandClappingGesture(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked && skeleton.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked)
            {
                float currentDistance = GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.HandLeft]);
                if (currentDistance < 0.1f && previousDistance > 0.1f)
                {
                    if (this.GestureRecognized != null)
                    {
                        this.GestureRecognized(this, new GestureEventArgs(RecognitionResult.Success));
                    }
                }

                previousDistance = currentDistance;
            }
        }
    }
}
