# serealistm

Adds current tech level stuff.
Gasses have somewhat mass.
space is hard.

For people who don't drink hydrogen peroxide.

# details

adds thermal management system
rebalances everything to modern phyiscs
adds electronic warfare, detecting emr

## gasses

## changed gasses
* Oxygen = Air = 4 N2 + 1 O2
* Hydrogen = Fuel = 2 H2 + 1 O2

## added gasses
* CO2
* H2
* O2
* N2
* Glucose = C6H12O6
* Biomass = Glucose + 6 Water
* Water = H2O
* Steam = H2O
* CarbonatedAir = 4 N2 + 1 CO2

## photosynthisis
6 CarbonatedAir + 6 Water => 6 Glucose + 6 Air

## changes
redefine "oxygen tank" to "air tank" stores 4 N2: 1 O2 mixture
redefine "hydrogen tank" to "fuel tank" stores 2 H2 : 1 O2 mixture
makes greenhouse do photosynthisis

makes hydrogen generator make sense. electrolysis takes a lot of energy, hydrogen engine produces ice as byproduct.

## blocks
gas mixer that does N2 + O2 => Air, H2 + O2 => Fuel
ice melter that does Ice => Water, removes heat if heat > 0
freezer that does Water => Ice, produces heat if heat < 0
condensor Steam => Water if heat < 100
release valve that dumps gasses
atmo radiator
space radiator