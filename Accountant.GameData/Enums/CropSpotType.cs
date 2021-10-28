namespace Accountant.Enums;

// A crop can be planted
// - inside a players apartment (2 pots, identified uniquely by tending player, the territory and their position, since you can not share apartment roommates)
// - inside a players personal chambers in his FC (same as apartment)
// - inside any house (2-4 pots depending on size, uniquely identified by world, territory, ward, plot and position.)
// - on a plot (1-3 patches, 1-8 beds per patch, uniquely identified by world, territory, ward, plot, patch and bed).
public enum CropSpotType : byte
{
    Invalid,
    Apartment,
    Chambers,
    House,
    Outdoors,
}
