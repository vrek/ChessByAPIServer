using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChessByAPIServer.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ChessBoard : ControllerBase
{
    // GET: api/<ChessBoard>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return ["value1", "value2"];
    }

    // GET api/<ChessBoard>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<ChessBoard>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<ChessBoard>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<ChessBoard>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
