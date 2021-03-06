﻿// The MIT License (MIT)
//
// Copyright (c) 2017 Henk-Jan Lebbink
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using AsmDude.Tools;

namespace AsmDude.QuickInfo
{
    internal sealed class AsmQuickInfoController : IIntellisenseController
    {
        private readonly IList<ITextBuffer> _subjectBuffers;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private IQuickInfoSession _session;
        private ITextView _textView;

        internal AsmQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, IQuickInfoBroker quickInfoBroker)
        {
            //AsmDudeToolsStatic.Output("INFO: AsmQuickInfoController:constructor: file=" + AsmDudeToolsStatic.GetFileName(textView.TextBuffer));
            this._textView = textView;
            this._subjectBuffers = subjectBuffers;
            this._quickInfoBroker = quickInfoBroker;
            this._textView.MouseHover += this.OnTextViewMouseHover;
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            //empty
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            //empty
        }

        public void Detach(ITextView textView)
        {
            if (this._textView == textView)
            {
                this._textView.MouseHover -= this.OnTextViewMouseHover;
                this._textView = null;
            }
        }

        /// <summary>
        /// Determine if the mouse is hovering over a token. If so, highlight the token and display QuickInfo
        /// </summary>
        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //AsmDudeToolsStatic.Output("INFO: AsmQuickInfoController:OnTextViewMouseHover: file=" + AsmDudeToolsStatic.GetFileName(this._textView.TextBuffer));
            SnapshotPoint? point = GetMousePosition(new SnapshotPoint(this._textView.TextSnapshot, e.Position));
            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);
                // Find the broker for this buffer
                if (!this._quickInfoBroker.IsQuickInfoActive(this._textView))
                {
                    //AsmDudeToolsStatic.Output("INFO: AsmQuickInfoController:OnTextViewMouseHover: CreateQuickInfoSession for triggerPoint "+triggerPoint.TextBuffer+"; file=" + AsmDudeToolsStatic.GetFileName(this._textView.TextBuffer));
                    this._session = this._quickInfoBroker.CreateQuickInfoSession(this._textView, triggerPoint, false);
                    this._session.Start();
                } else
                {
                    //AsmDudeToolsStatic.Output("INFO: AsmQuickInfoController:OnTextViewMouseHover: quickInfoBroker is already active; file=" + AsmDudeToolsStatic.GetFileName(this._textView.TextBuffer));
                }
            } else
            {
                //AsmDudeToolsStatic.Output("INFO: AsmQuickInfoController:OnTextViewMouseHover: point is null; file=" + AsmDudeToolsStatic.GetFileName(this._textView.TextBuffer));
            }
        }

        /// <summary>
        /// get mouse location on screen. Used to determine what word the cursor is currently hovering over
        /// </summary>
        private SnapshotPoint? GetMousePosition(SnapshotPoint topPosition)
        {
            // Map this point down to the appropriate subject buffer.

            return this._textView.BufferGraph.MapDownToFirstMatch(
                topPosition,
                PointTrackingMode.Positive,
                snapshot => this._subjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor
            );
        }
    }
}