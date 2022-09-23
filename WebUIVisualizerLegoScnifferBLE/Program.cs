using Newtonsoft.Json;
using System.IO.Pipes;
using System.Runtime.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

var client = new NamedPipeClientStream(LegoSnifferBLE.Program.mac_addres);
client.Connect();
StreamReader reader = new StreamReader(client);

Formatter = value => DateTime.Now.ToString("t");
Task.Factory.StartNew(() =>
{
    while (true)
    {
        string line = reader.ReadLine();
        try
        {
            dynamic stuff = JsonConvert.DeserializeObject(line);
           /* curr_cm_val = stuff["m_distance"];

            Values.Add(new DateTimePoint(DateTime.Now, Convert.ToDouble(curr_cm_val)));
            if (curr_cm_val > 4)
                Cm = curr_cm_val;
            if (Values.Count >= 100)
                m_values.RemoveAt(0);

            // }
            Dispatcher.CurrentDispatcher.Invoke(() =>
                SensorColor = Color.FromRgb((byte)stuff["m_rgb"][0], (byte)stuff["m_rgb"][1], (byte)stuff["m_rgb"][2]));

            Pitch = stuff["m_gyro"][0];
            Yaw = stuff["m_gyro"][1];
            Roll = stuff["m_gyro"][2];

            Startdate = DateTime.Now.AddSeconds(-5).Ticks;

            Rotation = new Vector3D(stuff["m_gyro"][0], stuff["m_gyro"][1], stuff["m_gyro"][2]);
           */
        }
        catch (Exception ex)
        {
            Console.Write("Bad message => Proceeding to next message");
        }
    }
});