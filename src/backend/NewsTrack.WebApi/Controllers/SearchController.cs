﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NewsTrack.Domain.Repositories;
using NewsTrack.WebApi.Dtos;

namespace NewsTrack.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly IDraftRepository _draftRepository;
        private readonly IMapper _mapper;

        public SearchController(IDraftRepository draftRepository, IMapper mapper)
        {
            _draftRepository = draftRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string query)
        {
            var drafts = await _draftRepository.Search(query);
            var dtos = _mapper.Map<IEnumerable<SearchResultDto>>(drafts);
            return Ok(dtos);
        }

        [HttpGet("advanced")]
        public async Task<IActionResult> Advanced(
            [FromQuery]string website,
            [FromQuery]string query, 
            [FromQuery]IEnumerable<string> tags, 
            [FromQuery]uint page,
            [FromQuery]uint count)
        {
            if (count > 0)
            {
                var news = await _draftRepository.Get(website, query, tags, (int)page, (int)count);
                var newsDtos = _mapper.Map<IEnumerable<NewsDto>>(news);

                var response = new NewsResponseListDto
                {
                    News = newsDtos,
                    Count = await _draftRepository.Count(website, query, tags)
                };

                return Ok(response);
            }

            return BadRequest();
        }     
    }
}