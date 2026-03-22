using System;
using System.Collections.Generic;
using System.IO;

namespace PortoesApp
{
    public class ConfigManager
    {
        private const string ConfigFile = "config.txt";
        private const string SerialConfigFile = "serial.cfg";

        // Lê as configurações dos portões do arquivo de texto
        public static List<GateConfig> LoadGateConfigs()
        {
            var gates = new List<GateConfig>();
            
            // Se o arquivo não existir, cria o arquivo com modelo padrão automaticamente
            if (!File.Exists(ConfigFile))
            {
                CreateDefaultConfig();
            }

            try
            {
                string[] lines = File.ReadAllLines(ConfigFile);
                foreach (string line in lines)
                {
                    // Ignora linhas vazias
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    string[] parts = line.Split(':');
                    
                    // Deve ter pelo menos 3 partes (ID:COMANDO:NOME)
                    if (parts.Length >= 3)
                    {
                        int id;
                        if (int.TryParse(parts[0], out id))
                        {
                            gates.Add(new GateConfig 
                            { 
                                Id = id, 
                                Command = parts[1], 
                                // Caso o nome contenha o caractere ':', agrupamos o restante da string
                                Name = string.Join(":", parts, 2, parts.Length - 2)
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Em caso de erro de leitura, ignora e retorna o que conseguiu ler sem travar o aplicativo
            }

            return gates;
        }

        // Gera o arquivo config.txt padrão
        private static void CreateDefaultConfig()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(ConfigFile))
                {
                    writer.WriteLine("1:1:Porta Principal");
                    writer.WriteLine("2:2:Porta Lateral");
                    writer.WriteLine("3:3:Entrada Caminhão");
                    for (int i = 4; i <= 16; i++)
                    {
                        string command = (i).ToString("X"); // Comando Hexadecimal genérico (ex: 4, 5, A, F...)
                        writer.WriteLine(string.Format("{0}:{1}:Portão {0}", i, command));
                    }
                }
            }
            catch (Exception)
            {
                // Ignora falhas de escrita (ex: falta de permissão na pasta)
            }
        }

        // Salva a configuração da porta serial e baud rate
        public static void SaveSerialConfig(string port, string baudRate)
        {
            try
            {
                File.WriteAllText(SerialConfigFile, string.Format("{0};{1}", port, baudRate));
            }
            catch (Exception) { /* Trata o erro silenciosamente */ }
        }

        // Carrega a configuração da porta serial gravada em acessos anteriores
        public static Tuple<string, string> LoadSerialConfig()
        {
            if (File.Exists(SerialConfigFile))
            {
                try
                {
                    string content = File.ReadAllText(SerialConfigFile);
                    string[] parts = content.Split(';');
                    if (parts.Length == 2)
                        return new Tuple<string, string>(parts[0], parts[1]);
                }
                catch (Exception) { }
            }
            // Valor padrão caso não exista configuração salva
            return new Tuple<string, string>("", "9600"); 
        }
    }
}
