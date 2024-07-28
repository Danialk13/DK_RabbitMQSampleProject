using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
var connection = factory.CreateConnection();
var model = connection.CreateModel();

Console.Write("Enter your username: ");
var username = Console.ReadLine();


Console.Write("Enter your friend's username: ");
var friendUsername = Console.ReadLine();

Console.Title = $"{username} -> {friendUsername}";

Console.Clear();

// Declaring a channel with your username to receive messages.
model.QueueDeclare(queue: username,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

// Creating a consumer and pass it to the model
var consumer = new EventingBasicConsumer(model);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[{friendUsername}]:  {message}");
};
model.BasicConsume(queue: username,
                     autoAck: true,
                     consumer: consumer);

// Creating a loop to send messages
while (true)
{
    var message = Console.ReadLine()!;
    if (message == "exit")
        break;
    var body = Encoding.UTF8.GetBytes(message);

    var localChannel = connection.CreateModel();

    // Publish messages to specific routingKey (friend's username)
    localChannel.BasicPublish(exchange: string.Empty,
                         routingKey: friendUsername,
                         basicProperties: null,
                         body: body);
}