﻿using System;
using System.Collections.Generic;

namespace NewsTrack.Data.Model
{
    public class DraftSuggestions : IDocument
    {
        public Guid Id { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<DraftReference> Drafts { get; set; }
    }
}