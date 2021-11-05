using System;
using System.Linq;
using Accountant.Enums;
using Accountant.SeFunctions;
using Accountant.Structs;
using Accountant.Timers;
using AddonWatcher;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using OtterLoc.Structs;

namespace Accountant.Manager;

public partial class TimerManager
{
    private readonly IAddonWatcher        _watcher;
    private          PositionInfoAddress? _positionInfo;

    internal string LastPlant = string.Empty;
    internal ushort LastPatch = ushort.MaxValue;
    internal ushort LastBed   = ushort.MaxValue;

    public void EnableCrops(bool state = true, bool force = false)
    {
        if (!force && state == Accountant.Config.EnableCrops)
            return;

        if (state)
        {
            CropTimers    ??= CropTimers.Load();
            _positionInfo ??= new PositionInfoAddress(Dalamud.SigScanner);
            _watcher.SubscribeTalkUpdate(CheckPlant);
            _watcher.SubscribeStringSelected(SelectStringEventDetour);
            _watcher.SubscribeYesnoSelected(SelectYesnoEventDetour);
        }
        else
        {
            _watcher.UnsubscribeYesnoSelected(SelectYesnoEventDetour);
            _watcher.UnsubscribeStringSelected(SelectStringEventDetour);
            _watcher.UnsubscribeTalkUpdate(CheckPlant);
            CropTimers = null;
        }

        if (!force)
        {
            Accountant.Config.EnableCrops = state;
            Accountant.Config.Save();
        }
    }

    public void CheckPlant(IntPtr talkPtr, SeString text, SeString speaker)
    {
        if (!StringId.CropDoingWell.Match(text) && !StringId.CropBetterDays.Match(text))
            return;

        var data = StringId.PlantMatcher.Filter(text);
        LastPlant = data.Count > 0 ? data[0] : string.Empty;
    }

    private void SetPatch(SeString description)
    {
        var data = StringId.PatchMatcher.Filter(description);
        if (data.Count == 2)
        {
            var patch = data[0][0] - '1';
            var bed   = data[1][0] - '1';
            if (patch < 3 && bed < 8)
            {
                LastPatch = (ushort)patch;
                LastBed   = (ushort)bed;
                return;
            }
        }

        LastPatch = ushort.MaxValue;
        LastBed   = ushort.MaxValue;
    }

    private CropSpotIdentification IdentifyCropSpot()
    {
        static CropSpotIdentification IdentifyCropSpotPrivate(CropSpotType type)
        {
            var target = Dalamud.Targets.Target;
            if (target == null)
                return CropSpotIdentification.Invalid;

            var ret = new CropSpotIdentification
            {
                Type       = type,
                PlayerName = Dalamud.ClientState.LocalPlayer!.Name.ToString(),
                ServerId   = (byte)Dalamud.ClientState.LocalPlayer.HomeWorld.Id,
                Position   = target.Position,
            };
            return ret;
        }

        CropSpotIdentification IdentifyCropSpotHouse()
        {
            var target = Dalamud.Targets.Target;
            if (target == null)
                return CropSpotIdentification.Invalid;

            var ret = new CropSpotIdentification
            {
                Type     = CropSpotType.House,
                ServerId = (byte)Dalamud.ClientState.LocalPlayer!.CurrentWorld.Id,
                Position = target.Position,
                Zone     = _positionInfo!.Zone,
                Ward     = (byte)_positionInfo.Ward,
                Plot     = (byte)_positionInfo.House,
            };
            if (ret.Zone == InternalHousingZone.Unknown || ret.Ward == 0 || ret.Plot == 0)
                return CropSpotIdentification.Invalid;

            return ret;
        }

        CropSpotIdentification IdentifyCropSpotOutdoor()
        {
            if (LastBed == ushort.MaxValue)
                return CropSpotIdentification.Invalid;

            var ret = new CropSpotIdentification
            {
                Type     = CropSpotType.Outdoors,
                ServerId = (byte)Dalamud.ClientState.LocalPlayer!.CurrentWorld.Id,
                Zone     = _positionInfo!.Zone,
                Ward     = (byte)_positionInfo.Ward,
                Plot     = _positionInfo.Plot,
                Bed      = (byte)LastBed,
                Patch    = (byte)LastPatch,
            };
            if (ret.Zone == InternalHousingZone.Unknown || ret.Ward == 0 || ret.Plot == 0 || ret.Bed > 7 || ret.Patch > 2)
                return CropSpotIdentification.Invalid;

            return ret;
        }

        switch ((HousingZone)Dalamud.ClientState.TerritoryType)
        {
            case HousingZone.Mist:
            case HousingZone.LavenderBeds:
            case HousingZone.Goblet:
            case HousingZone.Shirogane:
            case HousingZone.Firmament:
                return IdentifyCropSpotOutdoor();
            case HousingZone.ChambersMist:
            case HousingZone.ChambersLavenderBeds:
            case HousingZone.ChambersGoblet:
            case HousingZone.ChambersShirogane:
            case HousingZone.ChambersFirmament:
                return IdentifyCropSpotPrivate(CropSpotType.Chambers);
            case HousingZone.ApartmentMist:
            case HousingZone.ApartmentLavenderBeds:
            case HousingZone.ApartmentGoblet:
            case HousingZone.ApartmentShirogane:
            case HousingZone.ApartmentFirmament:
                return IdentifyCropSpotPrivate(CropSpotType.Apartment);
            case HousingZone.CottageMist:
            case HousingZone.CottageLavenderBeds:
            case HousingZone.CottageGoblet:
            case HousingZone.CottageShirogane:
            case HousingZone.CottageFirmament:
            case HousingZone.HouseMist:
            case HousingZone.HouseLavenderBeds:
            case HousingZone.HouseGoblet:
            case HousingZone.HouseShirogane:
            case HousingZone.HouseFirmament:
            case HousingZone.MansionMist:
            case HousingZone.MansionLavenderBeds:
            case HousingZone.MansionGoblet:
            case HousingZone.MansionShirogane:
            case HousingZone.MansionFirmament:
                return IdentifyCropSpotHouse();
            default:
                PluginLog.Error($"Housing Zone {Dalamud.ClientState.TerritoryType} should not be able to have crops.");
                return CropSpotIdentification.Invalid;
        }
    }

    public void SelectYesnoEventDetour(IntPtr atkUnit, bool yesOrNo, SeString buttonText, SeString descriptionText)
    {
        CropData GetCropData(string text)
        {
            var ret = StringId.SeedMatcher.Filter(text);
            return ret.Count == 0
                ? _gameData.FindCrop(0).Data
                : _gameData.FindCrop(ret[0]);
        }

        var newText = new SeString(descriptionText.Payloads.Where(p => p is not NewLinePayload).ToList());
        var text    = newText.TextValue;
        if (text.StartsWith(StringId.DisposeCrop.Value()))
        {
            var id = IdentifyCropSpot();
            if (id.Type != CropSpotType.Invalid)
                CropTimers!.HarvestCrop(id);
        }
        else
        {
            var itemId = GetCropData(text).Item.RowId;
            if (itemId == 0)
                return;

            var id = IdentifyCropSpot();
            if (id.Type != CropSpotType.Invalid)
                CropTimers!.PlantCrop(id, itemId, DateTime.UtcNow);
        }
    }

    private void SelectStringEventDetour(IntPtr unit, int which, SeString buttonText, SeString descriptionText)
    {
        switch (which)
        {
            case 0:
            {
                if (StringId.HarvestCrop.Match(buttonText))
                {
                    SetPatch(descriptionText);
                    var id = IdentifyCropSpot();
                    if (id.Type != CropSpotType.Invalid)
                        CropTimers!.HarvestCrop(id);
                }
                else if (StringId.PlantCrop.Match(buttonText)
                      || StringId.RemoveCrop.Match(buttonText))
                {
                    SetPatch(descriptionText);
                }

                return;
            }
            case 1:
            {
                if (!StringId.TendCrop.Match(buttonText))
                    return;

                SetPatch(descriptionText);
                var id = IdentifyCropSpot();
                if (id.Type != CropSpotType.Invalid)
                    CropTimers!.TendCrop(id, _gameData.FindCrop(LastPlant).Item.RowId, DateTime.UtcNow);
                return;
            }
            case 2:
            {
                if (StringId.RemoveCrop.Match(buttonText))
                    SetPatch(descriptionText);
                return;
            }
        }
    }
}
