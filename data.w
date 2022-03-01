atomFromName[name_]:=Atom[ElementData[name, "Abbreviation"]]

elements = {
    uranium = atomFromName["Uranium"],
    iron = atomFromName["Iron"],
    nickel = atomFromName["Nickel"],
    silicon = atomFromName["Silicon"],
    silver = atomFromName["Silver"],
    gold = atomFromName["Gold"],
    cobalt = atomFromName["Cobalt"],
    copper = atomFromName["Copper"],
    lithium = atomFromName["Lithium"],
    aluminum = atomFromName["Aluminum"],
    titanium = atomFromName["Titanium"],
    oxygen = atomFromName["Oxygen"],
    carbon = atomFromName["Carbon"],
    hydrogen = atomFromName["Hydrogen"],
    nitrogen = atomFromName["Nitrogen"],
    calcium = atomFromName["Calcium"],
    phosphorus = atomFromName["Phosphorus"],
    potassium = atomFromName["Potassium"],
    sulfur = atomFromName["Sulfur"],
    sodium = atomFromName["Sodium"],
    chlorine = atomFromName["Chlorine"],
    magnesium = atomFromName["Magnesium"],
    neodymium = atomFromName["Neodymium"],
    plutonium = atomFromName["Plutonium"],
}

(*https://www.researchgate.net/figure/Composition-of-essential-amino-acids-of-body-components-and-milk_tbl1_275753084*)
aminoAcids={
    "Alanine"->0.00,
    "Arginine"->0.065,
    "Asparagine"->0.00,
    "Aspartic acid"->0.00,
    "Cysteine"->0.011,
    "Glutamine"->0.00,
    "Glutamic acid"->0.00,
    "Glycine"->0.00,
    "Histidine"->0.037,
    "Isoleucine"->0.039,
    "Leucine"->0.071,
    "Lysine"->0.076,
    "Methionine"->0.019,
    "Phenylalanine"->0.038,
    "Proline"->0.00,
    "Serine"->0.0,
    "Threonine"->0.040,
    "Tryptophan"->0.011,
    "Tyrosine"->0.030,
    "Valine"->0.047,
};

chemical = Interpreter["Chemical"];
chemicalAtoms=chemical/*AtomList/*Total;

averageHumanMass=Entity["MedicalTest","Weight"][EntityProperty["MedicalTest","Mean"]];
averageHumanContent[Atom[symbol_]]:=UnitConvert[ElementData[symbol,"HumanAbundance"]/ElementData[symbol,"AtomicWeight"]*averageHumanMass/Quantity[1,"AvogadroConstant"]]

chemicals ={
    glucose = chemical["Glucose"],
    molecularHydrogen = chemical["Hydrogen"],
    molecularOxygen = chemical["Oxygen"],
    water = chemical["Water"],
    superconductor = chemical["Li2MgH16"],
}


volume[name_, phase_]:=QuantityMagnitude[ChemicalData[name,"MolarMass"]/ThermodynamicData[name,"TriplePoint"<>phase<>"Density"],"Meters"^3/"Moles"]

dump[name_]:=<|
"name"->name,
"volume"-><|
"gas"->Evaluate@volume[name, "Gas"],
"solid"->Evaluate@volume[name,"Solid"],
"liquid"->Evaluate@volume[name,"Liquid"],
|>
|>

human =24. oxygen + 12. carbon + 62. hydrogen + 1.1 nitrogen +0.038 sulfur;

water=chemicalAtoms["Water"];
carbonDioxide=chemicalAtoms["CarbonDioxide"];
glucose = chemicalAtoms["Glucose"];
aminoAcid=Expand@Mean[WeightedData[chemicalAtoms/@aminoAcids[[All,1]],aminoAcids[[All,2]]]];
fat=108hydrogen+54carbon+6oxygen;
air=4*nitrogen+oxygen;
biodiesel = 19 carbon + 34 hydrogen + 2 oxygen
(*methyl linoleate*)
elements={hydrogen,carbon,nitrogen,oxygen};
elementWeights={1,12,14,16};
parts={water,fat,glucose,aminoAcid};
vectorizeRules=Join[Thread[elements->IdentityMatrix[Length[elements]]],{sulfur->0}];
vectorize=ReplaceAll[vectorizeRules]/*N;
humanVector=vectorize@human;
partVectors=vectorize/@parts;
vectorMass[x_]:=x.elementWeights;
m=Transpose[Normalize[#,vectorMass]&/@partVectors];
c=vectorMass/@Transpose[m];
b=Normalize[humanVector,vectorMass];
ratio=Normalize[{0.5,0.2,0.2,0.1},Total];
flat=ratio*Min[b/(m.ratio)];
r=b-m.flat;

2 mole glucose => 1 mole agarose + 1 mole water

agarose=Expand[2 glucose-1 water];

6 parts water, 2 parts fat, 1 part agarose, 1 part amino acids


agarose=Expand[2glucose-1water];
cyclohexane=6 carbon + 12 hydrogen
biodiesel = 19 carbon + 34 hydrogen + 2 oxygen
Expand[8human-(2air+180water+4biodiesel+20 carbon)]
Expand[8human-(2air+190water+96carbon+116hydrogen)]
Expand[8human-(2air+8agarose+102water+116hydrogen)]
human->5 air+19 water+2 cyclohexane
human -> 31 water+12 carbon