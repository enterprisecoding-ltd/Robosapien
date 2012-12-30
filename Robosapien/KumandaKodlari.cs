using UsbUirt;
using System;

namespace Com.Enterprisecoding.Robosapien {
    internal static class KumandaKodlari {
        public static readonly CodeFormat KodFormati = CodeFormat.Uuirt;
        public static TimeSpan DoubleCommmandTimeSpan = new TimeSpan(0, 0, 2);

        #region Wake up / Sleep / Power off
        public static readonly string Sleep = "F5AR1BEC811578222022782220222022202278227822";

        public static readonly string WakeUp = "F41R206B81148083232323808323808323232323232323808323";

        public static readonly string PowerOff = "F41R29C881168084238084232323808423232323232323808423"; 
        #endregion

        public static readonly string Stop = "F3FR0EE681168085232423242324238085238085238085232423";

        #region Lean
        public static readonly string LeanRight = "F4AR13A1811572221F221F221F221F221F2272227122";

        public static readonly string LeanLeft = "F4DR0E51811872221F231F231F2372221F2372227122";

        public static readonly string LeanBack = "F4FR13BB811776222023752220232023762320237623";

        public static readonly string LeanForward = "F41R1FCD8114808323232380832323238083238083232323808323";
        #endregion

        #region Left Hand

        public static readonly string LeftArmIn = "F43R0EE181167D222122222221237D227D2222237D22";

        public static readonly string LeftArmOut = "F47R0DCF811476222022202220227622202276222022";

        public static readonly string LeftArmUp = "F40R0FF1811680832223232323232380832323232323808323";

        public static readonly string LeftArmDown = "F41R0E59811680812223232323232380812280812223232323";
        
        #endregion

        #region Move
        #region Walk
        public static readonly string MoveWalkLeft = "F3FR13228116808523242324232423808523242324232423";

        public static readonly string MoveWalkRight = "F3FR13A981168085232423242324232423242324232423";

        public static readonly string MoveWalkForward = "F3FR0EE3811680852324232423242324238085238085232423";

        public static readonly string MoveWalkBack = "F3FR0CC381168085232423242324232423808523808523808523";
        #endregion

        #region Step
        public static readonly string MoveStepForward = "F41R24B081148083232323808323232323238083238083232323";

        public static readonly string MoveStepBack = "F41R23A78114808323232380832323232323808323808323808323";

        public static readonly string MoveStepRight = "F41R22938114808323232380832323232323232323232323";

        public static readonly string MoveStepLeft  = "F41R20F5811480832323238083232323808323232323232323";
        #endregion 
        #endregion

        #region Right Hand
        public static readonly string RightArmIn = "F3FR0EE2811680852324232423242324238085232423808523";

        public static readonly string RightArmOut = "F3FR13208116808423232323232323232323238084232323";

        public static readonly string RightArmUp = "F3FR0DD38116808523242324232423242324232423808523";

        public static readonly string RightArmDown = "F3FR0FF28116808523242324232423242380852324232423";
        #endregion
    }
}