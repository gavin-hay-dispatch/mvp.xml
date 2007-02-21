#region using

using System;
using System.Collections;
using System.Xml.XPath;
using System.Xml;
using System.Text;
using System.Diagnostics; 

using Mvp.Xml.Common.XPath;

#endregion

namespace Mvp.Xml.XPointer 
{   
    /// <summary>
    /// element() scheme based <see cref="XPointer"/> pointer part.
    /// </summary>
    internal class ElementSchemaPointerPart : PointerPart 
    {
        #region private fields

        private string _NCName;
        private string _xpath;
        private ArrayList _childSequence;

        #endregion
		
        #region public members

        /// <summary>
        /// Leading NCName, see http://www.w3.org/TR/xptr-element/#model
        /// </summary>
        public string NCName 
        {
            get { return _NCName; }
            set { _NCName = value; }
        }
		
        /// <summary>
        /// Equivalent XPath expression.
        /// </summary>
        public string XPath 
        {
            get { return _xpath; }
            set { _xpath = value; }
        }
		
        /// <summary>
        /// ChildSequence as per http://www.w3.org/TR/xptr-element/#NT-ChildSequence
        /// </summary>
        public ArrayList ChildSequence 
        {
            get { return _childSequence; }
            set { _childSequence = value; }
        }
		
        #endregion

        #region PointerPart overrides

        /// <summary>
        /// Evaluates <see cref="XPointer"/> pointer part and returns pointed nodes.
        /// </summary>
        /// <param name="doc">Document to evaluate pointer part on</param>
        /// <param name="nm">Namespace manager</param>
        /// <returns>Pointed nodes</returns>
        public override XPathNodeIterator Evaluate(XPathNavigator doc, 
            XmlNamespaceManager nm) 
        {		    
            return XPathCache.Select(_xpath, doc, nm);
        }		
		
        #endregion

        #region parser

        /// <summary>
        /// Parses element() based pointer part and builds instance of <c>ElementSchemaPointerPart</c> class.
        /// </summary>
        /// <param name="lexer">Lexical analizer.</param>
        /// <returns>Newly created <c>ElementSchemaPointerPart</c> object.</returns>
        public static ElementSchemaPointerPart ParseSchemaData(XPointerLexer lexer) 
        {
            //Productions:
            //[1]   	ElementSchemeData	   ::=   	(NCName ChildSequence?) | ChildSequence
            //[2]   	ChildSequence	   ::=   	('/' [1-9] [0-9]*)+                        
            StringBuilder xpathBuilder = new StringBuilder();
            ElementSchemaPointerPart part = new ElementSchemaPointerPart();            
            lexer.NextLexeme();
            if (lexer.Kind == XPointerLexer.LexKind.NCName) 
            {
                part.NCName = lexer.NCName;                  
                xpathBuilder.Append("id('");
                xpathBuilder.Append(lexer.NCName);
                xpathBuilder.Append("')");
                lexer.NextLexeme();
            }                                                  
            part.ChildSequence = new ArrayList();
            while (lexer.Kind == XPointerLexer.LexKind.Slash) 
            {
                lexer.NextLexeme();
                if (lexer.Kind != XPointerLexer.LexKind.Number) 
                {
                    Debug.WriteLine(SR.InvalidTokenInElementSchemeWhileNumberExpected);
                    return null;
                }
                if (lexer.Number == 0) 
                {
                    Debug.WriteLine(SR.ZeroIndexInElementSchemechildSequence);
                    return null;    
                }                    
                part.ChildSequence.Add(lexer.Number);
                xpathBuilder.Append("/*[");
                xpathBuilder.Append(lexer.Number);
                xpathBuilder.Append("]");
                lexer.NextLexeme();
            }
            if (lexer.Kind != XPointerLexer.LexKind.RRBracket) 
            {                
                throw new XPointerSyntaxException(SR.InvalidTokenInElementSchemeWhileClosingRoundBracketExpected);
            }                               
            if (part.NCName==null && part.ChildSequence.Count==0) 
            {
                Debug.WriteLine(SR.EmptyElementSchemeXPointer);
                return null;         
            }
            part.XPath = xpathBuilder.ToString();            
            return part;
        }		
        #endregion
    }
}
