﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using NewsTrack.WebApi.IntegrationTests.Fixture;
using NewsTrack.WebApi.Dtos;

namespace NewsTrack.WebApi.IntegrationTests
{
    public class TopMostFuckingNewsEndpointTests : BaseTest
    {
        private const string Endpoint = "api/news/top/fucking";

        public TopMostFuckingNewsEndpointTests(TestWebAppFactory<Startup> testWebAppFactory) 
            : base(testWebAppFactory)
        {
        }

        [Fact]
        public async Task GivenExistingNews_WhenGettingTopMostFucking_ThenReturnsCollection()
        {
            // Arrange
            const uint take = 10;
            var draftResult = CreateDraftEntity();
            var result = new[] { draftResult };
            var endpoint = GetUriWithQueryString(Endpoint, new Tuple<string, object>("take", take));

            Factory.DraftRepositoryMock.Setup(m => m.GetMostFucking((int)take)).Returns(Task.FromResult(result.AsEnumerable()));

            // Act
            var response = await Client.GetAsync(endpoint.PathAndQuery);

            // Assert
            response.ShouldBeSuccessful();

            var envelope = await response.ShouldBeOfType<IEnumerable<NewsDigestDto>>();
            envelope.ShouldBeSuccessful();

            envelope.Payload.Should().HaveCount(result.Length);
            var dto = envelope.Payload.First();
            AssertDigestDto(draftResult, dto);
        }

        [Fact]
        public async Task GivenInvalidRequestWithNonPositiveTake_WhenGettingTopMostFucking_ThenReturnsBadRequest()
        {
            // Arrange
            var endpoint = GetUriWithQueryString(Endpoint, new Tuple<string, object>("take", 0));

            // Act
            var response = await Client.GetAsync(endpoint.PathAndQuery);

            // Assert
            response.ShouldBeBadRequest();
        }

        [Fact]
        public async Task GivenInvalidRequestWithoutTake_WhenGettingTopMostFucking_ThenReturnsBadRequest()
        {
            // Act
            var response = await Client.GetAsync(Endpoint);

            // Assert
            response.ShouldBeBadRequest();
        }
    }
}