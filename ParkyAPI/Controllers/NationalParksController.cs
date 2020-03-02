using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Dtos;
using ParkyAPI.Models;
using ParkyAPI.Repository;

namespace ParkyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ParkyOpenAPISpecNP")]
    [ProducesResponseType(400)]
    public class NationalParksController : ControllerBase
    {
        private readonly INationalParkRepository _npRepo;
        private readonly IMapper _mapper;

        public NationalParksController(INationalParkRepository npRepo, IMapper mapper)
        {
            _npRepo = npRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a list of all national parks.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<NationalParkDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetNationalParks()
        {
            var objList = _npRepo.GetNationalPark();

            //This will ensure we send our Dto instead of the model
            var objDto = new List<NationalParkDto>();
            foreach (var obj in objList)
                objDto.Add(_mapper.Map<NationalParkDto>(obj));

            return Ok(objDto);
        }

        /// <summary>
        /// Get individual national park
        /// </summary>
        /// <param name="nationalParkId">The national park Id</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{nationalParkId:int}", Name = "GetNationalPark")]
        [ProducesResponseType(200, Type = typeof(NationalParkDto))]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        public IActionResult GetNationalPark(int nationalParkId)
        {
            var obj = _npRepo.GetNationalPark(nationalParkId);

            //This will ensure we send our Dto instead of the model
            var objDto = new List<NationalParkDto>();

            if (obj == null)
                return NotFound();

             objDto.Add(_mapper.Map<NationalParkDto>(obj));

            return Ok(objDto);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(NationalParkDto))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult CreateNationalPark([FromBody] NationalParkDto nationalParkDto)
        {
            //ModelState contains all related encountered errors
            if (nationalParkDto == null)
                return BadRequest(ModelState);

            if (_npRepo.NationalParkExists(nationalParkDto.Name))
            {
                //Give the ModelState a message and an error code 404 to return to the client
                ModelState.AddModelError("", "National Park Exists!");
                return StatusCode(404, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Map parameter Dto to the Model
            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);

            //If creation was unsuccessful
            if (!_npRepo.CreateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetNationalPark", new { nationalParkId = nationalParkObj.Id }, nationalParkObj);
        }

        [HttpPatch("{nationalParkId:int}", Name = "UpdateNationalPark")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult UpdateNationalPark(int nationalParkId, [FromBody] NationalParkDto nationalParkDto)
        {
            //ModelState contains all related encountered errors
            if (nationalParkDto == null || nationalParkId != nationalParkDto.Id)
                return BadRequest(ModelState);

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);

            //If update was unsuccessful
            if (!_npRepo.UpdateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{nationalParkId:int}", Name = "DeleteNationalPark")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {
            //ModelState contains all related encountered errors
            if (!_npRepo.NationalParkExists(nationalParkId))
                return NotFound();

            var nationalParkObj = _npRepo.GetNationalPark(nationalParkId);

            //If update was unsuccessful
            if (!_npRepo.DeleteNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}