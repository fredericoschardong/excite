using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TimelineSample
{
    class ReadProximityEvents
    {
        private String file;
        private BinaryReader reader;
        private List <String> entities = new List <String>();
        private List <Package> packages = new List <Package>();

        private DateTime firstDate;

        public ReadProximityEvents(String file)
        {
            if (File.Exists(file))
            {
                this.file = file;
                FileStream fs = File.OpenRead(file);
                reader = new BinaryReader(fs);

                int size = reader.ReadInt32();

                for (int x = 0; x < size; x++)
                {
                    entities.Add("");

                    while (true)
                    {
                        char next = reader.ReadChar();

                        if (next != '|')
                        {
                            if (char.IsLetterOrDigit(next))
                            {
                                entities[x] += next;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    
                    entities[x] = entities[x].Trim().ToLower();
                }

                string time = "";

                while (true)
                {
                    char next = reader.ReadChar();

                    if (next != '^')
                    {
                        if (char.IsLetterOrDigit(next) || char.IsPunctuation(next) || next == ' ')
                        {
                            time += next;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                this.firstDate = DateTime.Parse(time);

                //entities.Add("-1");
            }
            else
            {
                Console.WriteLine("file not found");
                Environment.Exit(0);
            }
        }

        public DateTime getFirstDate()
        {
            return this.firstDate;
        }

        public List<Package> readAll()
        {
            try
            {
                while (this.getNext() != null) ;
            }
            catch (EndOfStreamException e)
            {

            }

            return packages;
        }

        public List<Package> getPackages()
        {
            return this.packages;
        }

        public List<String> getEntities()
        {
            return this.entities;
        }

        public String getFile()
        {
            return this.file;
        }

        public Package getNext()
        {
            try
            {
                int type = reader.ReadInt32();

                switch (type)
                {
                    case 0:
                        this.packages.Add(new PackageCollision(reader));
                        break;
                    case 1:
                        this.packages.Add(new PackagePointing(reader, entities));
                        break;
                    case 2:
                        this.packages.Add(new PackageDirection(reader, entities));
                        break;
                    case 3:
                        this.packages.Add(new PackageLocation(reader, entities));
                        break;
                    case 4:
                        this.packages.Add(new PackageMotion(reader, entities));
                        break;
                    default:
                        Console.Out.WriteLine("Unknown type: " + type + " at: " + this.packages.Count);
                        Environment.Exit(0);
                        return null;
                }

                return this.packages[this.packages.Count - 1];
            }
            catch (EndOfStreamException e)
            {
                return null;
            }
        }
    }
}
