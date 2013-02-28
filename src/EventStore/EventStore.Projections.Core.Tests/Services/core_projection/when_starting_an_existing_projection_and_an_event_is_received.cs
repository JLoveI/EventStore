// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Text;
using EventStore.Core.Data;
using EventStore.Projections.Core.Messages;
using EventStore.Projections.Core.Services.Processing;
using NUnit.Framework;
using ResolvedEvent = EventStore.Projections.Core.Services.Processing.ResolvedEvent;

namespace EventStore.Projections.Core.Tests.Services.core_projection
{
    [TestFixture]
    public class when_starting_an_existing_projection_and_an_event_is_received : TestFixtureWithCoreProjectionStarted
    {
        private string _testProjectionState = @"{""test"":1}";

        protected override void Given()
        {
            ExistingEvent(
                "$projections-projection-result", "Result",
                @"{""CommitPosition"": 100, ""PreparePosition"": 50}", _testProjectionState);
            ExistingEvent(
                "$projections-projection-checkpoint", "ProjectionCheckpoint",
                @"{""CommitPosition"": 100, ""PreparePosition"": 50}", _testProjectionState);
            ExistingEvent(
                "$projections-projection-result", "Result",
                @"{""CommitPosition"": 200, ""PreparePosition"": 150}", _testProjectionState);
            ExistingEvent(
                "$projections-projection-result", "Result",
                @"{""CommitPosition"": 300, ""PreparePosition"": 250}", _testProjectionState);
            NoStream("$projections-projection-order");
            AllWritesToSucceed("$projections-projection-order");
        }

        protected override void When()
        {
            var eventId = Guid.NewGuid();
            _coreProjection.Handle(
                ProjectionSubscriptionMessage.CommittedEventReceived.Sample(
                    Guid.Empty, _subscriptionId, new EventPosition(120, 110), "/event_category/1", -1, false,
                    ResolvedEvent.Sample(
                        eventId, "append", false, Encoding.UTF8.GetBytes("data"), Encoding.UTF8.GetBytes("metadata")), 0));
        }


        [Test]
        public void should_load_projection_state_handler()
        {
            Assert.AreEqual(1, _stateHandler._loadCalled);
            Assert.AreEqual(_testProjectionState + "data", _stateHandler._loadedState);
        }
    }
}
