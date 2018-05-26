using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.ServiceBus;
using Dapper.Contrib.Extensions;

namespace APICotacoes.Controllers
{
    [Route("api/[controller]")]
    public class CotacoesController : Controller
    {
        [HttpGet]
        public IEnumerable<Cotacao> GetCotacoes(
            [FromServices]IConfiguration configuration)
        {
            using (SqlConnection conexao = new SqlConnection(
                configuration.GetConnectionString("BaseCotacoes")))
            {
                return conexao.GetAll<Cotacao>();
            }
        }

        [HttpGet("carregar")]
        public object CarregarCotacoes(
            [FromServices]ServiceBusConfigurations configurations)
        {
            string conteudo = "Solicitação de Carga - " +
                $"API Cotacoes - {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
            var body = Encoding.UTF8.GetBytes(conteudo);
            
            var client = new QueueClient(
                configurations.ConnectionString,
                configurations.QueueName,
                ReceiveMode.ReceiveAndDelete);
            client.SendAsync(new Message(body)).Wait();

            return new
            {
                Resultado = "Mensagem encaminhada com sucesso"
            };
        }
    }
}