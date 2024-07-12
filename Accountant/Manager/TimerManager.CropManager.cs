using System;
using System.Linq;
using Accountant.Enums;
using Accountant.Gui.Timer;
using Accountant.SeFunctions;
using Accountant.Structs;
using Accountant.Timers;
using AddonWatcher;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterLoc.Structs;

namespace Accountant.Manager;

public partial class TimerManager
{
    private sealed class CropManager : ITimerManager
    {
        public ConfigFlags RequiredFlags
            => ConfigFlags.Enabled | ConfigFlags.Crops;

        private readonly IAddonWatcher       _watcher;
        private readonly PositionInfoAddress _positionInfo;
        private readonly IGameData           _gameData;

        private bool   _state;
        private string _lastPlant = string.Empty;
        private ushort _lastPatch = ushort.MaxValue;
        private ushort _lastBed   = ushort.MaxValue;

        private readonly PlotCropTimers    _plotCrops;
        private readonly PrivateCropTimers _privateCrops;

        public CropManager(PlotCropTimers plotCrops, PrivateCropTimers privateCrops, PositionInfoAddress positionInfo)
        {
            _plotCrops    = plotCrops;
            _privateCrops = privateCrops;
            _positionInfo = positionInfo;
            _watcher      = Accountant.Watcher;
            _gameData     = Accountant.GameData;
            SetState();
        }

        public TimerWindow.BaseCache CreateCache(TimerWindow window)
            => new TimerWindow.CropCache(window, RequiredFlags, _plotCrops, _privateCrops);

        public void SetState()
        {
            if (Accountant.Config.Flags.Check(RequiredFlags))
                Enable();
            else
                Disable();
        }

        private void Enable()
        {
            if (_state)
                return;

            _plotCrops.Reload();
            _privateCrops.Reload();
            _watcher.SubscribeTalkUpdate(CheckPlant);
            _watcher.SubscribeStringSelected(SelectStringEventDetour);
            _watcher.SubscribeYesnoSelected(SelectYesnoEventDetour);
            _state = true;
        }

        private void Disable()
        {
            _watcher.UnsubscribeYesnoSelected(SelectYesnoEventDetour);
            _watcher.UnsubscribeStringSelected(SelectStringEventDetour);
            _watcher.UnsubscribeTalkUpdate(CheckPlant);
            _state = false;
        }

        public void Dispose()
            => Disable();

        private void CheckPlant(IntPtr talkPtr, SeString text, SeString speaker)
        {
            if (!StringId.CropDoingWell.Match(text) && !StringId.CropBetterDays.Match(text))
                return;

            var data = StringId.PlantMatcher.Filter(text);
            _lastPlant = data.Count > 0 ? data[0] : string.Empty;
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
                    _lastPatch = (ushort)patch;
                    _lastBed   = (ushort)bed;
                    return;
                }
            }

            _lastPatch = ushort.MaxValue;
            _lastBed   = ushort.MaxValue;
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
                    ServerId   = (ushort)Dalamud.ClientState.LocalPlayer.HomeWorld.Id,
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
                    ServerId = (ushort)Dalamud.ClientState.LocalPlayer!.CurrentWorld.Id,
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
                if (_lastBed == ushort.MaxValue)
                    return CropSpotIdentification.Invalid;

                var ret = new CropSpotIdentification
                {
                    Type     = CropSpotType.Outdoors,
                    ServerId = (ushort)Dalamud.ClientState.LocalPlayer!.CurrentWorld.Id,
                    Zone     = _positionInfo!.Zone,
                    Ward     = (byte)_positionInfo.Ward,
                    Plot     = _positionInfo.Plot,
                    Bed      = (byte)_lastBed,
                    Patch    = (byte)_lastPatch,
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
                case HousingZone.Empyreum:
                    return IdentifyCropSpotOutdoor();
                case HousingZone.ChambersMist:
                case HousingZone.ChambersLavenderBeds:
                case HousingZone.ChambersGoblet:
                case HousingZone.ChambersShirogane:
                case HousingZone.ChambersEmpyreum:
                    return IdentifyCropSpotPrivate(CropSpotType.Chambers);
                case HousingZone.ApartmentMist:
                case HousingZone.ApartmentLavenderBeds:
                case HousingZone.ApartmentGoblet:
                case HousingZone.ApartmentShirogane:
                case HousingZone.ApartmentEmpyreum:
                    return IdentifyCropSpotPrivate(CropSpotType.Apartment);
                case HousingZone.CottageMist:
                case HousingZone.CottageLavenderBeds:
                case HousingZone.CottageGoblet:
                case HousingZone.CottageShirogane:
                case HousingZone.CottageEmpyreum:
                case HousingZone.HouseMist:
                case HousingZone.HouseLavenderBeds:
                case HousingZone.HouseGoblet:
                case HousingZone.HouseShirogane:
                case HousingZone.HouseEmpyreum:
                case HousingZone.MansionMist:
                case HousingZone.MansionLavenderBeds:
                case HousingZone.MansionGoblet:
                case HousingZone.MansionShirogane:
                case HousingZone.MansionEmpyreum:
                    return IdentifyCropSpotHouse();
                default:
                    Dalamud.Log.Error($"Housing Zone {Dalamud.ClientState.TerritoryType} should not be able to have crops.");
                    return CropSpotIdentification.Invalid;
            }
        }

        private void SelectYesnoEventDetour(IntPtr atkUnit, bool yesOrNo, SeString buttonText, SeString descriptionText)
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
                switch (id.Type)
                {
                    case CropSpotType.Apartment:
                    case CropSpotType.Chambers:
                        _privateCrops.HarvestCrop(id);
                        break;
                    case CropSpotType.Outdoors:
                    case CropSpotType.House:
                        _plotCrops.HarvestCrop(id);
                        break;
                }
            }
            else
            {
                var itemId = GetCropData(text).Item.RowId;
                if (itemId == 0)
                    return;

                var id = IdentifyCropSpot();
                switch (id.Type)
                {
                    case CropSpotType.Apartment:
                    case CropSpotType.Chambers:
                        _privateCrops.PlantCrop(id, itemId, DateTime.UtcNow);
                        break;
                    case CropSpotType.Outdoors:
                    case CropSpotType.House:
                        _plotCrops.PlantCrop(id, itemId, DateTime.UtcNow);
                        break;
                }
            }
        }

        private void HarvestCrop(SeString descriptionText)
        {
            SetPatch(descriptionText);
            var id = IdentifyCropSpot();
            switch (id.Type)
            {
                case CropSpotType.Apartment:
                case CropSpotType.Chambers:
                    _privateCrops.HarvestCrop(id);
                    break;
                case CropSpotType.Outdoors:
                case CropSpotType.House:
                    _plotCrops.HarvestCrop(id);
                    break;
            }
        }

        private void TendCrop(SeString descriptionText)
        {
            SetPatch(descriptionText);
            var id = IdentifyCropSpot();
            switch (id.Type)
            {
                case CropSpotType.Apartment:
                case CropSpotType.Chambers:
                    _privateCrops.TendCrop(id, _gameData.FindCrop(_lastPlant).Item.RowId, DateTime.UtcNow);
                    break;
                case CropSpotType.Outdoors:
                case CropSpotType.House:
                    _plotCrops.TendCrop(id, _gameData.FindCrop(_lastPlant).Item.RowId, DateTime.UtcNow);
                    break;
            }
        }

        private void SelectStringEventDetour(IntPtr unit, int which, SeString buttonText, SeString descriptionText)
        {
            switch (which)
            {
                case 0:
                {
                    if (StringId.HarvestCrop.Match(buttonText))
                        HarvestCrop(descriptionText);
                    else if (StringId.PlantCrop.Match(buttonText))
                        SetPatch(descriptionText);
                    else if (StringId.RemoveCrop.Match(buttonText))
                        SetPatch(descriptionText);
                    else if (StringId.TendCrop.Match(buttonText))
                        TendCrop(descriptionText);
                    return;
                }
                case 1:
                {
                    if (StringId.TendCrop.Match(buttonText))
                        TendCrop(descriptionText);
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
}
