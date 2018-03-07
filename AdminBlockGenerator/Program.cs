using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AdminBlockGenerator
{
    class Program
    {
        private string fileDirectory;
        private string[] filesInDirectory;

        public Program(string path)
        {
            fileDirectory = path;

            if (Directory.Exists(path))
            {
                filesInDirectory = Directory.GetFiles(path);

                ConvertCubeBlock();
                ConvertBlockCategories();
                ConvertEntityContainers();
                ConvertEntityComponents();

            }
            else
            {
                Console.WriteLine("Error: Path did not exist");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ConvertEntityComponents()
        {
            string path = Path.Combine(fileDirectory, "EntityComponents.sbc");

            if (!filesInDirectory.Contains(path))
            {
                Console.WriteLine($"Could not locate {path}");
                return;
            }

            try
            {
                XDocument document = XDocument.Load(path);
                IEnumerable<XElement> components = document.Root.XPathSelectElements("//EntityComponent");

                foreach (XElement component in components)
                {
                    //XElement typeId = component.Element("Id").Element("TypeId");
                    //typeId.Value = typeId.Value + "_Admin";

                    XElement subtype = component.Element("Id").Element("SubtypeId");
                    if (subtype != null)
                    {
                        subtype.Value = (subtype.Value.EndsWith("_Admin")) ? subtype.Value : subtype.Value + "_Admin";
                    }
                }

                document.Save("./EntityComponents.sbc");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }

        private void ConvertEntityContainers()
        {
            string path = Path.Combine(fileDirectory, "EntityContainers.sbc");

            if (!filesInDirectory.Contains(path))
            {
                Console.WriteLine($"Could not locate {path}");
                return;
            }

            try
            {
                XDocument document = XDocument.Load(path);
                IEnumerable<XElement> containers = document.Root.XPathSelectElements("//Container");

                foreach (XElement container in containers)
                {

                    //XElement typeId = container.Element("Id").Element("TypeId");
                    //typeId.Value = typeId.Value + "_Admin";

                    XElement subtype = container.Element("Id").Element("SubtypeId");
                    if (subtype != null)
                    {
                        subtype.Value = (subtype.Value.EndsWith("_Admin")) ? subtype.Value : subtype.Value + "_Admin";
                    }
                }

                document.Save("./EntityContainers.sbc");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }

        private void ConvertBlockCategories()
        {
            string path = Path.Combine(fileDirectory, "BlockCategories.sbc");

            if (!filesInDirectory.Contains(path))
            {
                Console.WriteLine($"Could not locate {path}");
                return;
            }

            try
            {
                XDocument document = XDocument.Load(path);
                document.Root.XPathSelectElements("//Category").Remove();
                //IEnumerable<XElement> categories = document.Root.XPathSelectElements("//Category");

                //foreach (XElement category in categories)
                //{
                //    XElement displayName = category.Element("DisplayName");
                //    //displayName.Value = (displayName.Value.EndsWith("_Admin")) ? displayName.Value : displayName.Value + "_Admin";

                //    XElement name = category.Element("Name");
                //    name.Value = (name.Value.EndsWith("_Admin")) ? name.Value : name.Value + "_Admin";
                //    displayName.Value = name.Value;

                //    foreach (XElement item in category.Element("ItemIds").Elements())
                //    {
                //        item.Value = (item.Value.EndsWith("_Admin")) ? item.Value : item.Value + "_Admin";
                //        //string[] pair = item.Value.Split('/');

                //        //if (pair.Length == 2)
                //        //{
                //        //    item.Value = pair[0] + "_Admin/" + pair[1] + "_Admin";
                //        //}
                //        //else
                //        //{
                //        //    item.Value = pair[0];
                //        //}
                //    }
                //}

                document.Save("./BlockCategories.sbc");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }

        private void ConvertCubeBlock()
        {

            string path = Path.Combine(fileDirectory, "CubeBlocks.sbc");

            if (!filesInDirectory.Contains(path))
            {
                Console.WriteLine($"Error: Could not locate {path}");
                return;
            }

            try
            {
                Console.WriteLine($"Loading {path}");
                XDocument document = XDocument.Load(path);
                XElement steelplate = new XElement("Component", new XAttribute("Subtype", "SteelPlate"), new XAttribute("Count", "1"));
                XElement computer = new XElement("Component", new XAttribute("Subtype", "Computer"), new XAttribute("Count", "1"));
                XElement adminkit = new XElement("Component", new XAttribute("Subtype", "AdminKit"), new XAttribute("Count", "1"));
                XElement adminkit2 = new XElement("Component", new XAttribute("Subtype", "AdminKit"), new XAttribute("Count", "1"));

                IEnumerable<XElement> definitions = document.Root.XPathSelectElements("//Definition");
                foreach (XElement definition in definitions)
                {
                    XElement displayName = definition.Element("DisplayName");
                    Console.WriteLine($"Editing {displayName?.Value}");
                    XElement typeId = definition.Element("Id").Element("TypeId");
                    XElement subtype = definition.Element("Id").Element("SubtypeId");
                    subtype.Value = (subtype.Value.EndsWith("_Admin")) ? subtype.Value : subtype.Value + "_Admin";
                    displayName.Value = subtype.Value;

                    XElement blockPairName = definition.Element("BlockPairName");
                    if (blockPairName != null)
                    {
                        blockPairName.Value = (blockPairName.Value.EndsWith("_Admin")) ? blockPairName.Value : blockPairName.Value + "_Admin";
                    }

                    XElement guiVisible = definition.Element("GuiVisible");
                    if (guiVisible == null)
                    {
                        definition.Add(new XElement("GuiVisible", "false"));
                    }
                    else
                    {
                        guiVisible.Value = "false";
                    }

                    string[] ignoreComputersForTypes = new string[] { "CubeBlock", "Conveyor", "Wheel", "ConveyorConnector", "MotorRotor", "MotorAdvancedRotor", "PistonTop", "Passage" };
                    XElement components = definition.Element("Components");

                    components.RemoveAll();
                    //components.AddFirst(adminkit);
                    components.Add(adminkit);
                    if (!ignoreComputersForTypes.Contains(typeId.Value))
                    {
                        components.Add(computer);
                    }
                    components.Add(steelplate);
                    components.Add(adminkit2);

                    XElement criticalComponent = definition.Element("CriticalComponent");
                    criticalComponent.Attribute("Subtype").Value = "AdminKit";
                    criticalComponent.Attribute("Index").Value = "1";

                    XElement buildTime = definition.Element("BuildTimeSeconds");
                    if (buildTime == null)
                    {
                        definition.Add(new XElement("BuildTimeSeconds", float.MaxValue.ToString()));
                    }
                    else
                    {
                        buildTime.Value = float.MaxValue.ToString();
                    }

                    definition.Add(new XElement("UsesDeformation", "false"));

                    XElement deformationRatio = definition.Element("DeformationRatio");
                    if (deformationRatio == null)
                    {
                        definition.Add(new XElement("DeformationRatio", "0"));
                    }
                    else
                    {
                        deformationRatio.Value = "0";
                    }

                    XElement generalDamageReduction = definition.Element("GeneralDamageMultiplier");
                    if (generalDamageReduction == null)
                    {
                        definition.Add(new XElement("GeneralDamageMultiplier", "0"));
                    }

                    XElement disassembleRatio = definition.Element("DisassembleRatio");

                    if (disassembleRatio == null)
                    {
                        definition.Add(new XElement("DisassembleRatio", float.MaxValue.ToString()));
                    }
                    else
                    {
                        disassembleRatio.Value = float.MaxValue.ToString();
                    }

                    XElement blockVariants = definition.Element("BlockVariants");
                    if (blockVariants != null)
                    {
                        foreach (XElement variant in blockVariants.Elements())
                        {
                            XElement subtype2 = variant.Element("SubtypeId");
                            subtype2.Value = subtype2.Value.EndsWith("_Admin") ? subtype2.Value : subtype2.Value + "_Admin";
                        }
                    }
                }


                IEnumerable<XElement> blockPositions = document.Root.XPathSelectElements("//BlockPosition");

                foreach (XElement block in blockPositions)
                {
                    XElement name = block.Element("Name");
                    name.Value = name.Value.EndsWith("_Admin") ? name.Value : name.Value + "_Admin";
                }

                document.Save("./CubeBlocks.sbc");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program(args[0]);
        }
    }
}
