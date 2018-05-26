using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Azure.ServiceBus;

namespace CarregarCotacoes
{
    class Program
    {
        private static IConfiguration _configuration;
        private static SeleniumConfigurations _seleniumConfigurations;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");
            _configuration = builder.Build();

            var serviceBusConfigurations = new ServiceBusConfigurations();
            new ConfigureFromConfigurationOptions<ServiceBusConfigurations>(
                _configuration.GetSection("ServiceBusConfigurations"))
                    .Configure(serviceBusConfigurations);

            _seleniumConfigurations = new SeleniumConfigurations();
            new ConfigureFromConfigurationOptions<SeleniumConfigurations>(
                _configuration.GetSection("SeleniumConfigurations"))
                    .Configure(_seleniumConfigurations);


            var client = new QueueClient(
                serviceBusConfigurations.ConnectionString,
                serviceBusConfigurations.QueueName,
                ReceiveMode.ReceiveAndDelete);
            try
            {
                client.RegisterMessageHandler(
                       async (message, token) =>
                       {
                           ProcessarCargaCotacoes(message);
                       },
                       new MessageHandlerOptions(
                           async (e) =>
                           {
                               Console.WriteLine("[Erro] " +
                                   e.Exception.GetType().FullName + " " +
                                   e.Exception.Message);
                           }
                       )
                );

                Console.ReadKey();
            }
            finally
            {
                client.CloseAsync().Wait();
            }
        }

        private static void ProcessarCargaCotacoes(
            Message message)
        {
            var conteudo = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine(Environment.NewLine +
                "[Nova mensagem recebida] " + conteudo);

            List<Cotacao> cotacoes;
            PaginaCotacoes pagina =
                new PaginaCotacoes(_seleniumConfigurations);
            try
            {
                Console.WriteLine("Iniciando extração dos dados...");
                pagina.CarregarPagina();
                cotacoes = pagina.ObterCotacoes();
                Console.WriteLine("Dados extraídos com sucesso!");

                new CotacoesDAO(_configuration.GetConnectionString("BaseCotacoes"))
                    .CarregarDados(cotacoes);
                Console.WriteLine("Carga dos dados efetuada com sucesso!");
            }
            finally
            {
                pagina.Fechar();
            }
        }
    }
}