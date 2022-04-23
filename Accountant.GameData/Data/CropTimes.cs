namespace Accountant.Data;

public partial class Crops
{
    private static readonly (int GrowthTime, int WiltTime, int ItemId, int SeedId)[] Data = new[]
    {
        (0 * 00, 00, 00000, 00000), // Nothing
        (5 * 24, 48, 04835, 07738), // Ala Mhigan Mustard
        (7 * 24, 24, 30873, 30364), // Allagan Melon
        (5 * 24, 48, 04842, 07744), // Almond
        (1 * 22, 72, 15857, 15855), // Althyk Lavender
        (6 * 24, 36, 07592, 07751), // Apricot
        (6 * 24, 36, 07769, 07746), // Azeyma Rose
        (5 * 24, 48, 04830, 07737), // Black Pepper
        (5 * 24, 48, 04814, 07730), // Blood Currant
        (7 * 24, 24, 13754, 13755), // Blood Pepper
        (7 * 24, 24, 07776, 07757), // Broombrush
        (5 * 24, 48, 05542, 07740), // Chamomile
        (5 * 24, 48, 12884, 13766), // Chive
        (2 * 24, 72, 08162, 08179), // Cieldalaes Pineapple
        (1 * 24, 72, 17548, 17547), // Cloud Acorn
        (5 * 24, 48, 20791, 20792), // Cloudsbreath
        (5 * 24, 48, 04778, 07717), // Coerthan Carrot
        (5 * 24, 48, 12882, 13768), // Coerthan Tea
        (5 * 24, 48, 07894, 08169), // Curiel Root
        (6 * 24, 36, 07603, 07749), // Dalamud Popoto Set
        (2 * 24, 72, 08158, 08175), // Doman Plum
        (5 * 24, 48, 06148, 07724), // Dzemael Tomato
        (1 * 18, 72, 00005, 15868), // Earth Shard
        (7 * 24, 24, 08193, 08185), // Eggplant Knight
        (5 * 24, 48, 04810, 07727), // Fairie Apple
        (1 * 18, 72, 00002, 15865), // Firelight
        (5 * 24, 48, 07735, 07735), // Garlic Cloves
        (7 * 24, 24, 08194, 08186), // Garlic Jester
        (7 * 24, 24, 07775, 07756), // Glazenut
        (3 * 24, 24, 04868, 08572), // Gysahl Greens
        (6 * 24, 36, 07768, 07745), // Halone Gerbera
        (2 * 24, 72, 08163, 08180), // Han Lemon
        (5 * 24, 48, 06147, 07733), // Honey Lemon
        (1 * 18, 72, 00003, 15866), // Icelight
        (7 * 24, 24, 07774, 07755), // Jute
        (3 * 24, 24, 08165, 08182), // Krakka Root
        (6 * 24, 36, 07593, 07752), // La Noscean Leek
        (5 * 24, 48, 04782, 07718), // La Noscean Lettuce
        (5 * 24, 48, 04809, 07725), // La Noscean Orange
        (5 * 24, 48, 05539, 07736), // Lavender
        (1 * 18, 72, 00006, 15869), // Levinlight
        (5 * 24, 48, 05346, 07741), // Linseed
        (5 * 24, 48, 04808, 07726), // Lowland Grape
        (2 * 24, 72, 08159, 08176), // Mamook Pear
        (7 * 24, 24, 08196, 08188), // Mandragora Queen
        (5 * 24, 48, 05543, 07743), // Mandrake
        (5 * 24, 48, 04837, 07742), // Midland Basil
        (5 * 24, 48, 04789, 07723), // Midland Cabbage
        (5 * 24, 48, 04821, 07721), // Millioncorn
        (5 * 24, 48, 07897, 08171), // Mimett Gourd
        (5 * 24, 48, 06146, 07731), // Mirror Apple
        (6 * 24, 36, 07770, 07747), // Nymeia Lily
        (2 * 24, 72, 08161, 08178), // O'Ghomoro Berry
        (5 * 24, 48, 12896, 13765), // Old World Fig
        (5 * 24, 48, 04804, 07719), // Olive
        (7 * 24, 24, 08192, 08184), // Onion Prince
        (5 * 24, 48, 07900, 08173), // Pahsana Fruit
        (5 * 24, 48, 04785, 07715), // Paprika
        (5 * 24, 48, 04836, 07739), // Pearl Ginger Root
        (6 * 24, 36, 08023, 08167), // Pearl Roselle
        (5 * 24, 48, 12877, 13767), // Pearl Sprout
        (5 * 24, 48, 04812, 07729), // Pixie Plum
        (5 * 24, 48, 04787, 07720), // Popoto Set
        (5 * 24, 48, 04816, 07734), // Prickly Pineapple
        (5 * 24, 48, 04815, 07732), // Rolanberry
        (5 * 24, 24, 20793, 20794), // Royal Fern
        (6 * 24, 36, 07604, 07750), // Royal Kukuru
        (6 * 24, 36, 07591, 07753), // Shroud Tea
        (6 * 24, 36, 07602, 07748), // Star Anise
        (5 * 24, 48, 04811, 07728), // Sun Lemon
        (5 * 24, 48, 07895, 08170), // Sylkis Bud
        (5 * 24, 48, 07898, 08172), // Tantalplant
        (5 * 48, 24, 08166, 08183), // Thavnairian Onion
        (7 * 24, 24, 08195, 08187), // Tomato King
        (7 * 24, 24, 07773, 07754), // Umbrella Fig
        (2 * 24, 72, 08160, 08177), // Valfruit
        (1 * 12, 72, 15858, 15856), // Voidrake
        (1 * 18, 72, 00007, 15870), // Waterlight
        (5 * 24, 48, 04777, 07716), // Wild Onion Set
        (1 * 18, 72, 00004, 15867), // Windlight
        (5 * 24, 48, 04788, 07722), // Wizard Eggplant
        (2 * 24, 72, 08157, 08174), // Xelphatol Apple
    };
}
