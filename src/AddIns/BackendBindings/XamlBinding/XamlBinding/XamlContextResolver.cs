﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Core;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Xml;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Parser;

namespace ICSharpCode.XamlBinding
{
	/// <summary>
	/// Collects data needed to generate completion and insigth info.
	/// </summary>
	public class XamlContextResolver
	{
		public static XamlContext ResolveContext(FileName fileName, string text, int offset)
		{
			return ResolveContext(fileName, new StringTextSource(text), offset);
		}
		
		public static XamlContext ResolveContext(FileName fileName, ITextSource fileContent, int offset)
		{
			XamlFullParseInformation info = SD.ParserService.Parse(fileName, fileContent) as XamlFullParseInformation;
			
			if (info == null)
				throw new Exception("need full parse info!");
			
			AXmlDocument document = info.Document;
			AXmlObject currentData = document.GetChildAtOffset(offset);
			
			string attributeName = string.Empty, attributeValue = string.Empty;
			AttributeValue value = null;
			bool isRoot = false;
			bool wasAXmlElement = false;
			int offsetFromValueStart = -1;
			
			List<AXmlElement> ancestors = new List<AXmlElement>();
			Dictionary<string, XamlNamespace> xmlns = new Dictionary<string, XamlNamespace>();
			List<string> ignored = new List<string>();
			string xamlNamespacePrefix = string.Empty;
			
			var item = currentData;
			
			while (item != document) {
				if (item is AXmlElement) {
					AXmlElement element = item as AXmlElement;
					ancestors.Add(element);
					foreach (var attr in element.Attributes) {
						if (attr.IsNamespaceDeclaration) {
							string prefix = (attr.Name == "xmlns") ? "" : attr.LocalName;
							if (!xmlns.ContainsKey(prefix))
								xmlns.Add(prefix, new XamlNamespace(attr.Value));
						}
						
						if (attr.LocalName == "Ignorable" && attr.Namespace == XamlConst.MarkupCompatibilityNamespace)
							ignored.AddRange(attr.Value.Split(' ', '\t'));
						
						if (string.IsNullOrEmpty(xamlNamespacePrefix) && attr.Value == XamlConst.XamlNamespace)
							xamlNamespacePrefix = attr.LocalName;
					}
					
					if (!wasAXmlElement && item.Parent is AXmlDocument)
						isRoot = true;
					
					wasAXmlElement = true;
				}
				
				item = item.Parent;
			}
			
			XamlContextDescription description = XamlContextDescription.None;
			
			AXmlElement active = null;
			AXmlElement parent = null;
			
			if (currentData is AXmlAttribute) {
				AXmlAttribute a = currentData as AXmlAttribute;
				int valueStartOffset = a.ValueSegment.Offset + 1;
				attributeName = a.Name;
				attributeValue = a.Value;
				value = MarkupExtensionParser.ParseValue(attributeValue);
				
				if (offset >= valueStartOffset && offset < a.EndOffset) {
					offsetFromValueStart = offset - valueStartOffset;
					
					description = XamlContextDescription.InAttributeValue;
					
					if (value != null && !value.IsString)
						description = XamlContextDescription.InMarkupExtension;
					if (attributeValue.StartsWith("{}", StringComparison.Ordinal) && attributeValue.Length > 2)
						description = XamlContextDescription.InAttributeValue;
				} else
					description = XamlContextDescription.InTag;
			} else if (currentData is AXmlTag) {
				AXmlTag tag = currentData as AXmlTag;
				if (tag.IsStartOrEmptyTag || tag.IsEndTag) {
					if (tag.NameSegment.EndOffset < offset)
						description = XamlContextDescription.InTag;
					else
						description = XamlContextDescription.AtTag;
				} else if (tag.IsComment)
					description = XamlContextDescription.InComment;
				else if (tag.IsCData)
					description = XamlContextDescription.InCData;
				active = tag.Parent as AXmlElement;
			}
			
			if (active != ancestors.FirstOrDefault())
				parent = ancestors.FirstOrDefault();
			else
				parent = ancestors.Skip(1).FirstOrDefault();
			
			if (active == null)
				active = parent;
			
			var xAttribute = currentData as AXmlAttribute;
			
			var context = new XamlContext() {
				Description         = description,
				ActiveElement       = active,
				ParentElement       = parent,
				Ancestors           = ancestors.AsReadOnly(),
				Attribute           = xAttribute,
				InRoot              = isRoot,
				AttributeValue      = value,
				RawAttributeValue   = attributeValue,
				ValueStartOffset    = offsetFromValueStart,
				XmlnsDefinitions    = xmlns,
				ParseInformation    = info,
				IgnoredXmlns        = ignored.AsReadOnly(),
				XamlNamespacePrefix = xamlNamespacePrefix
			};
			
			return context;
		}
		
		public static XamlCompletionContext ResolveCompletionContext(ITextEditor editor, char typedValue)
		{
			var binding = editor.GetService(typeof(XamlLanguageBinding)) as XamlLanguageBinding;
			
			if (binding == null)
				throw new InvalidOperationException("Can only use ResolveCompletionContext with a XamlLanguageBinding.");
			
			var context = new XamlCompletionContext(ResolveContext(editor.FileName, editor.Document.CreateSnapshot(), editor.Caret.Offset)) {
				PressedKey = typedValue,
				Editor = editor
			};
			
			return context;
		}
		
		public static XamlCompletionContext ResolveCompletionContext(ITextEditor editor, char typedValue, int offset)
		{
			var binding = editor.GetService(typeof(XamlLanguageBinding)) as XamlLanguageBinding;
			
			if (binding == null)
				throw new InvalidOperationException("Can only use ResolveCompletionContext with a XamlLanguageBinding.");
			
			var context = new XamlCompletionContext(ResolveContext(editor.FileName, editor.Document.CreateSnapshot(), offset)) {
				PressedKey = typedValue,
				Editor = editor
			};
			
			return context;
		}
	}
}