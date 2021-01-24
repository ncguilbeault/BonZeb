﻿namespace BonZeb
{
    public class TailKinematicsData
    {
        // Class used for creating a data type which contains the amplitudes, frequency, and instances of bouts in tail curvature data.
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public bool Instance { get; set; }
        public TailKinematicsData(double frequency, double amplitude, bool instance)
        {
            Frequency = frequency;
            Amplitude = amplitude;
            Instance = instance;
        }
    }
}
