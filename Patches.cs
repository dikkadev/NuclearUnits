using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace NuclearUnits
{
    [HarmonyPatch(typeof(Altitude), nameof(Altitude.Refresh))]
    internal static class AltitudePatch
    {
        private static readonly AccessTools.FieldRef<Altitude, Text> RadarAltRef = AccessTools.FieldRefAccess<Altitude, Text>("radarAlt");
        private static readonly AccessTools.FieldRef<Altitude, Text> AbsAltRef = AccessTools.FieldRefAccess<Altitude, Text>("absAlt");
        private static readonly AccessTools.FieldRef<Altitude, Aircraft> AircraftRef = AccessTools.FieldRefAccess<Altitude, Aircraft>("aircraft");

        private static void Postfix(Altitude __instance)
        {
            var aircraft = AircraftRef(__instance);
            if (aircraft == null)
            {
                return;
            }

            RadarAltRef(__instance).text = "R[" + HudUnitFormatter.FormatAltitude(aircraft.radarAlt, Plugin.Instance.AltitudeHudRadarAlt.Value) + "]";
            AbsAltRef(__instance).text = HudUnitFormatter.FormatAltitude(aircraft.transform.position.GlobalY(), Plugin.Instance.AltitudeHudAbsoluteAlt.Value);
        }
    }

    [HarmonyPatch(typeof(SpeedGauge), nameof(SpeedGauge.Refresh))]
    internal static class SpeedGaugePatch
    {
        private static readonly AccessTools.FieldRef<SpeedGauge, Text> AirspeedDisplayRef = AccessTools.FieldRefAccess<SpeedGauge, Text>("airspeedDisplay");
        private static readonly AccessTools.FieldRef<SpeedGauge, Aircraft> AircraftRef = AccessTools.FieldRefAccess<SpeedGauge, Aircraft>("aircraft");

        private static void Postfix(SpeedGauge __instance)
        {
            var aircraft = AircraftRef(__instance);
            if (aircraft == null)
            {
                return;
            }

            AirspeedDisplayRef(__instance).text = HudUnitFormatter.FormatSpeed(aircraft.speed, Plugin.Instance.SpeedGaugeAirspeed.Value);
        }
    }

    [HarmonyPatch(typeof(BasicFlightInstruments), nameof(BasicFlightInstruments.Refresh))]
    internal static class BasicFlightInstrumentsPatch
    {
        private static readonly AccessTools.FieldRef<BasicFlightInstruments, Aircraft> AircraftRef = AccessTools.FieldRefAccess<BasicFlightInstruments, Aircraft>("aircraft");
        private static readonly AccessTools.FieldRef<BasicFlightInstruments, Text> AirspeedDisplayRef = AccessTools.FieldRefAccess<BasicFlightInstruments, Text>("airspeedDisplay");
        private static readonly AccessTools.FieldRef<BasicFlightInstruments, Text> AltitudeDisplayRef = AccessTools.FieldRefAccess<BasicFlightInstruments, Text>("altitudeDisplay");
        private static readonly AccessTools.FieldRef<BasicFlightInstruments, Text> ClimbRateDisplayRef = AccessTools.FieldRefAccess<BasicFlightInstruments, Text>("climbRateDisplay");

        private static void Postfix(BasicFlightInstruments __instance)
        {
            var aircraft = AircraftRef(__instance);
            if (aircraft == null)
            {
                return;
            }

            AirspeedDisplayRef(__instance).text = HudUnitFormatter.FormatSpeed(aircraft.speed, Plugin.Instance.BasicInstrumentsAirspeed.Value);
            AltitudeDisplayRef(__instance).text = HudUnitFormatter.FormatAltitude(aircraft.radarAlt, Plugin.Instance.BasicInstrumentsAltitude.Value);

            var climbRate = Vector3.Dot(aircraft.CockpitRB().velocity, Vector3.up);
            ClimbRateDisplayRef(__instance).text = HudUnitFormatter.FormatClimbRate(climbRate, Plugin.Instance.BasicInstrumentsClimbRate.Value);
        }
    }

    [HarmonyPatch(typeof(Climbrate), nameof(Climbrate.Refresh))]
    internal static class ClimbratePatch
    {
        private static readonly AccessTools.FieldRef<Climbrate, Text> ClimbRateRef = AccessTools.FieldRefAccess<Climbrate, Text>("climbRate");
        private static readonly AccessTools.FieldRef<Climbrate, Aircraft> AircraftRef = AccessTools.FieldRefAccess<Climbrate, Aircraft>("aircraft");

        private static void Postfix(Climbrate __instance)
        {
            var aircraft = AircraftRef(__instance);
            if (aircraft == null)
            {
                return;
            }

            var climbRate = Vector3.Dot(aircraft.CockpitRB().velocity, Vector3.up);
            ClimbRateRef(__instance).text = HudUnitFormatter.FormatClimbRate(climbRate, Plugin.Instance.ClimbrateHudValue.Value);
        }
    }

    [HarmonyPatch(typeof(VirtualMFD), "Update")]
    internal static class VirtualMfdPatch
    {
        private static readonly AccessTools.FieldRef<VirtualMFD, Text> SpeedRef = AccessTools.FieldRefAccess<VirtualMFD, Text>("speed");
        private static readonly AccessTools.FieldRef<VirtualMFD, Text> AltitudeRef = AccessTools.FieldRefAccess<VirtualMFD, Text>("altitude");

        private static void Postfix(VirtualMFD __instance)
        {
            if (!DynamicMap.mapMaximized || SceneSingleton<CombatHUD>.i.aircraft == null)
            {
                return;
            }

            var aircraft = SceneSingleton<CombatHUD>.i.aircraft;
            SpeedRef(__instance).text = HudUnitFormatter.FormatSpeed(aircraft.speed, Plugin.Instance.VirtualMfdSpeed.Value);
            AltitudeRef(__instance).text = HudUnitFormatter.FormatAltitude(aircraft.radarAlt, Plugin.Instance.VirtualMfdAltitude.Value);
        }
    }

    [HarmonyPatch(typeof(LandingScreenUI), "LateUpdate")]
    internal static class LandingScreenPatch
    {
        private static readonly AccessTools.FieldRef<LandingScreenUI, Text> AltitudeRef = AccessTools.FieldRefAccess<LandingScreenUI, Text>("altitude");
        private static readonly AccessTools.FieldRef<LandingScreenUI, Text> VerticalSpeedRef = AccessTools.FieldRefAccess<LandingScreenUI, Text>("vert_speed");
        private static readonly AccessTools.FieldRef<LandingScreenUI, Text> SpeedRef = AccessTools.FieldRefAccess<LandingScreenUI, Text>("speed");
        private static readonly AccessTools.FieldRef<LandingScreenUI, Text> RelativeSpeedRef = AccessTools.FieldRefAccess<LandingScreenUI, Text>("rel_speed");

        private static void Postfix(LandingScreenUI __instance)
        {
            var aircraft = SceneSingleton<CombatHUD>.i.aircraft;
            if (aircraft == null)
            {
                return;
            }

            AltitudeRef(__instance).text = HudUnitFormatter.FormatAltitudeLabel("ALT ", aircraft.radarAlt, Plugin.Instance.LandingScreenAltitude.Value);
            SpeedRef(__instance).text = HudUnitFormatter.FormatSpeedLabel("SPD ", aircraft.speed, Plugin.Instance.LandingScreenSpeed.Value);
            VerticalSpeedRef(__instance).text = HudUnitFormatter.FormatClimbRateLabel("V ", aircraft.rb.velocity.y, Plugin.Instance.LandingScreenVerticalSpeed.Value);

            var horizontalSpeed = new Vector3(aircraft.rb.velocity.x, 0f, aircraft.rb.velocity.z).magnitude;
            RelativeSpeedRef(__instance).text = HudUnitFormatter.FormatSpeedLabel("H ", horizontalSpeed, Plugin.Instance.LandingScreenHorizontalSpeed.Value);
        }
    }

    [HarmonyPatch(typeof(TargetMarker), "ExtraSetup")]
    internal static class TargetMarkerSetupPatch
    {
        private static void Postfix(TargetMarker __instance)
        {
            TargetMarkerCommon.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(TargetMarker), "Update")]
    internal static class TargetMarkerUpdatePatch
    {
        private static void Postfix(TargetMarker __instance)
        {
            TargetMarkerCommon.Apply(__instance);
        }
    }

    internal static class TargetMarkerCommon
    {
        private static readonly AccessTools.FieldRef<TargetMarker, Text> InfoRangeRef = AccessTools.FieldRefAccess<TargetMarker, Text>("infoRange");
        private static readonly AccessTools.FieldRef<TargetMarker, Text> InfoSpeedRef = AccessTools.FieldRefAccess<TargetMarker, Text>("infoSpeed");
        private static readonly AccessTools.FieldRef<TargetMarker, Text> InfoAltRef = AccessTools.FieldRefAccess<TargetMarker, Text>("infoAlt");

        internal static void Apply(TargetMarker marker)
        {
            var unit = marker.GetUnit();
            if (unit == null)
            {
                return;
            }

            if (InfoSpeedRef(marker).text != "SPD -")
            {
                InfoSpeedRef(marker).text = HudUnitFormatter.FormatSpeedLabel("SPD ", unit.speed, Plugin.Instance.TargetMarkerSpeed.Value);
            }

            if (unit is Aircraft)
            {
                InfoAltRef(marker).text = HudUnitFormatter.FormatAltitudeLabel("ALT ", unit.radarAlt, Plugin.Instance.TargetMarkerAltitude.Value);
            }
            else if (unit is Missile)
            {
                InfoAltRef(marker).text = HudUnitFormatter.FormatAltitudeLabel("ALT ", unit.GlobalPosition().y, Plugin.Instance.TargetMarkerAltitude.Value);
            }

            if (SceneSingleton<CombatHUD>.i.aircraft != null && InfoRangeRef(marker).text != "RNG -")
            {
                var distance = FastMath.Distance(SceneSingleton<CombatHUD>.i.aircraft.GlobalPosition(), unit.GlobalPosition());
                InfoRangeRef(marker).text = HudUnitFormatter.FormatDistanceLabel("RNG ", distance, Plugin.Instance.TargetMarkerRange.Value);
            }
        }
    }

    [HarmonyPatch(typeof(TargetScreenUI), "UpdateTargetInfo")]
    internal static class TargetScreenPatch
    {
        private static readonly AccessTools.FieldRef<TargetScreenUI, Text> DistanceRef = AccessTools.FieldRefAccess<TargetScreenUI, Text>("distance");
        private static readonly AccessTools.FieldRef<TargetScreenUI, Text> AltitudeRef = AccessTools.FieldRefAccess<TargetScreenUI, Text>("altitude");
        private static readonly AccessTools.FieldRef<TargetScreenUI, Text> RelativeAltitudeRef = AccessTools.FieldRefAccess<TargetScreenUI, Text>("rel_altitude");
        private static readonly AccessTools.FieldRef<TargetScreenUI, Text> SpeedRef = AccessTools.FieldRefAccess<TargetScreenUI, Text>("speed");
        private static readonly AccessTools.FieldRef<TargetScreenUI, Text> RelativeSpeedRef = AccessTools.FieldRefAccess<TargetScreenUI, Text>("rel_speed");
        private static readonly AccessTools.FieldRef<TargetScreenUI, List<Unit>> TargetListRef = AccessTools.FieldRefAccess<TargetScreenUI, List<Unit>>("targetList");
        private static readonly AccessTools.FieldRef<TargetScreenUI, TargetCam> TargetCamRef = AccessTools.FieldRefAccess<TargetScreenUI, TargetCam>("targetCam");
        private static readonly AccessTools.FieldRef<TargetScreenUI, FactionHQ> HqRef = AccessTools.FieldRefAccess<TargetScreenUI, FactionHQ>("hq");

        private static void Postfix(TargetScreenUI __instance)
        {
            var targetList = TargetListRef(__instance);
            var targetCam = TargetCamRef(__instance);
            var hq = HqRef(__instance);
            var localAircraft = SceneSingleton<CombatHUD>.i.aircraft;

            if (targetList == null || targetCam == null || hq == null || localAircraft == null || targetList.Count == 0)
            {
                return;
            }

            DistanceRef(__instance).text = HudUnitFormatter.FormatDistanceLabel("RNG ", targetCam.GetDist(), Plugin.Instance.TargetScreenDistance.Value);

            if (targetList.Count > 1)
            {
                return;
            }

            var target = targetList[0];
            var hasAccuratePosition = hq.IsTargetPositionAccurate(target, 20f);
            var hasDetailedReadout = target is Aircraft || target is Missile;
            if (!hasAccuratePosition || !hasDetailedReadout)
            {
                return;
            }

            var targetPosition = target.GlobalPosition();
            var relative = targetPosition - localAircraft.GlobalPosition();
            AltitudeRef(__instance).text = HudUnitFormatter.FormatAltitudeLabel("ALT ", targetPosition.y, Plugin.Instance.TargetScreenAltitude.Value);
            RelativeAltitudeRef(__instance).text = HudUnitFormatter.FormatAltitudeLabel("REL ", relative.y, Plugin.Instance.TargetScreenRelativeAltitude.Value);
            SpeedRef(__instance).text = HudUnitFormatter.FormatSpeedLabel("SPD ", target.speed, Plugin.Instance.TargetScreenSpeed.Value);

            var relativeSpeed = Vector3.Dot(localAircraft.rb.velocity, relative.normalized) - Vector3.Dot(target.rb.velocity, relative.normalized);
            RelativeSpeedRef(__instance).text = HudUnitFormatter.FormatSpeedLabel("REL ", relativeSpeed, Plugin.Instance.TargetScreenRelativeSpeed.Value);
        }
    }
}
