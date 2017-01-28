
//--------------------------------------------------------------------------------------------------
/// ������� � ������ ��� ������ � ������� XML �������
//
/// ���������: ������ ��������
/// 2012 ���
//-----------------------------------------------------------------------------------------------


#ifndef INCLUDE__XML_H
#define INCLUDE__XML_H

#include <string.h>
#include <stdlib.h>

#include "_Macro.h"

#include "_Xml.h"


//������� � ������ (xml) �������� ��� (tag) � �������� ��� ���������� � ��������� �����
//���������� 1, ���� ����������
//TODO: ����������, ���������
char Xml_getNValue(const char *xml, const char *tag, unsigned int tagN, char *buffer, size_t buffer_length)
{
	char *pBegin;
	char *pEnd;
	char begin_tag[50];
	char end_tag[50];

	strcpy(begin_tag, "<");
	strcat(begin_tag, tag);
	strcat(begin_tag, ">");

	strcpy(end_tag, "</");
	strcat(end_tag, tag);
	strcat(end_tag, ">");

	pBegin = (char*)strstr(xml, begin_tag);

	while(pBegin != 0)
	{
		pBegin += strlen(begin_tag);

		if(tagN-- != 0)
		{
			//���� ��������� ��������� ������� ����
			pBegin = (char*)strstr(pBegin, begin_tag);

		}
		else
		{
			pEnd = (char*)strstr(pBegin, end_tag);

			if(pEnd != 0)
			{
				size_t value_length = pEnd - pBegin;

				if(value_length + 1 > buffer_length) // +1 ��� ������������ ������
				{
					// ����� ����� ��������� ������ >:(
					return 0;

				}

				strncpy(buffer, pBegin, value_length);

				buffer[value_length] = 0;

				return 1;

			}
			else
			{
				DEBUG("����� xml �� ������", 0);

			}

		}

	}

	return 0;

}

int XmlGetValueBoundaries(const char *pXmlStart, const char *pXmlEnd, const char *pTag, unsigned int n, char **ppFoundValueStart, size_t *pValueLength)
{
	char *pBegin;
	char *pEnd;
	char beginTag[50];
	char endTag[50];

	strcpy(beginTag, "<");
	strcat(beginTag, pTag);
	strcat(beginTag, ">");

	strcpy(endTag, "</");
	strcat(endTag, pTag);
	strcat(endTag, ">");

	pBegin = (char*)strstr(pXmlStart, beginTag);

	while(pBegin != 0 && pBegin <= pXmlEnd)
	{
		//������ ��������� �� ������� "����� ����� ��������� ���������� ����"
		pBegin += strlen(beginTag);

		if(n-- != 0)
		{
			//���� ��������� ��������� ���������� ����
			pBegin = (char*)strstr(pBegin, beginTag);

		}
		else
		{
			pEnd = (char*)strstr(pBegin, endTag);

			//���� �� ����� �� ������������ ������ � �� ����� �� ������ ������� �������� ������� xml
			if(pEnd != 0 && pEnd <= pXmlEnd)
			{
				*ppFoundValueStart = pBegin;
				if(pValueLength != NULL)
				{
					*pValueLength = pEnd - pBegin;

				}

				return 0;

			}

		}

	}

	return -1;

}

int XmlGetValue(const char *pXmlStart, const char *pXmlEnd, const char *pTag, unsigned int n, char *valueBuffer, size_t bufferLength)
{
	char *pValueStart;
	size_t valueLength;

	if(0 == XmlGetValueBoundaries(pXmlStart, pXmlEnd, pTag, n, &pValueStart, &valueLength))
	{
		if(bufferLength >= valueLength + 1)
		{
			strncpy(valueBuffer, pValueStart, valueLength);
			
			valueBuffer[valueLength] = 0;

			return 0;

		}

	}

	return -1;

}

int XmlGetValueAsInt(const char *pXmlStart, const char *pXmlEnd, const char *pTag, unsigned int n, int* pValue)
{
	char valueBuffer[20];
	char *pValueStart;
	size_t valueLength;

	if(0 == XmlGetValue(pXmlStart, pXmlEnd, pTag, n, valueBuffer, sizeof(valueBuffer)))
	{
		*pValue = atoi(valueBuffer);

		return 0;

	}

	return -1;

}

int XmlGetValueAsLong(const char *pXmlStart, const char *pXmlEnd, const char *pTag, unsigned int n, long* pValue)
{
	char valueBuffer[20];
	char *pValueStart;
	size_t valueLength;

	if(0 == XmlGetValue(pXmlStart, pXmlEnd, pTag, n, valueBuffer, sizeof(valueBuffer)))
	{
		//ShowMessage(valueBuffer, "������� Enter");

		*pValue = atol(valueBuffer);

		//sprintf(valueBuffer, "%ld", *pValue);

		//ShowMessage(valueBuffer, "������� Enter (2)");

		return 0;

	}

	return -1;

}


//-----------------------------------------------


void XmlAsTable_Constructor(struct XmlAsTable* pXmlAsTable, const char* pRecordsetStart, const char* pRecordsetEnd, const char *rowTag)
{
	strncpy(pXmlAsTable->RowTag, rowTag, sizeof(pXmlAsTable->RowTag));
	pXmlAsTable->pRecordsetStart = pRecordsetStart;
	pXmlAsTable->pRecordsetEnd = pRecordsetEnd;

	//��������� �� ������ ������ �������
	XmlAsTable_MoveFirst(pXmlAsTable);

}

int XmlAsTable_MoveFirst(struct XmlAsTable* pXmlAsTable)
{
	return
		XmlGetValueBoundaries
		(
			pXmlAsTable->pRecordsetStart,
			pXmlAsTable->pRecordsetEnd,
			pXmlAsTable->RowTag,
			0,
			&pXmlAsTable->pCurrentRowValue,
			NULL// � ����� � ������ �������� �� ���������
		);

}

int XmlAsTable_MoveNext(struct XmlAsTable* pXmlAsTable)
{
	//��� ������� �������������, �� �������� �����
	return
		XmlGetValueBoundaries
		(
			pXmlAsTable->pCurrentRowValue,
			pXmlAsTable->pRecordsetEnd,
			pXmlAsTable->RowTag,
			0,
			&pXmlAsTable->pCurrentRowValue,
			NULL// � ����� � ������ �������� �� ���������
		);

}

int XmlAsTable_GetValue(struct XmlAsTable* pXmlAsTable, const char *tag, char *valueBuffer, size_t bufferLength)
{
	return
		XmlGetValue
		(
			pXmlAsTable->pCurrentRowValue,
			pXmlAsTable->pRecordsetEnd,
			tag,
			0,
			valueBuffer,
			bufferLength
		);

}

int XmlAsTable_GetValueAsInt(struct XmlAsTable* pXmlAsTable, const char *tag, int *pValue)
{
	return
		XmlGetValueAsInt
		(
			pXmlAsTable->pCurrentRowValue,
			pXmlAsTable->pRecordsetEnd,
			tag,
			0,
			pValue
		);

}

int XmlAsTable_GetValueAsLong(struct XmlAsTable* pXmlAsTable, const char *tag, long *pValue)
{
	return
		XmlGetValueAsLong
		(
			pXmlAsTable->pCurrentRowValue,
			pXmlAsTable->pRecordsetEnd,
			tag,
			0,
			pValue
		);

}

int XmlToTable(const char *pXmlStart, const char *pXmlEnd, const char *rowTag, struct DataTableDefinition *pTable, void (*rowConverter)(const char*, const char*, void*))
{
	struct XmlAsTable xmlTable;
//	int itemCount;

	XmlAsTable_Constructor(&xmlTable, pXmlStart, pXmlEnd, rowTag);

	if(0 == XmlAsTable_MoveFirst(&xmlTable))
	{
		int exitFlag = 0;

		for(pTable->RowNumber = 0; pTable->RowNumber < pTable->Capacity && exitFlag == 0; pTable->RowNumber++)
		{
			rowConverter(xmlTable.pCurrentRowValue, xmlTable.pRecordsetEnd, DataTable_GetRowPtr(pTable, pTable->RowNumber));
			//XmlAsTable_GetValue(&xmlTable, "productid", pList[itemCount].Artikul, sizeof(pList[itemCount].Artikul));

			if(0 != XmlAsTable_MoveNext(&xmlTable))
			{
				exitFlag = 1;

			}

		}

//		pTable->RowNumber = itemCount;

	}
	else
	{
		pTable->RowNumber = 0;

	}

	return 0;

}

//�������� & �� &amp;  < �� &lt;  > �� &gt;  " �� &quot;  ' �� &apos;
char* XmlCh(const char *string, char *result, size_t resultBufferLength)
{
	size_t len = strlen(string);

	const char *psrc = string;
	char *pdest = result;

	size_t i;

	*pdest = 0;

	for(i = 0; i < len; i ++)
	{
		if(*psrc == '&')
		{
			*(pdest++) = '&';
			*(pdest++) = 'a';
			*(pdest++) = 'm';
			*(pdest++) = 'p';
			*pdest = ';';

		}
		else if(*psrc == '<')
		{
			*(pdest++) = '&';
			*(pdest++) = 'l';
			*(pdest++) = 't';
			*pdest = ';';

		}
		else if(*psrc == '>')
		{
			*(pdest++) = '&';
			*(pdest++) = 'g';
			*(pdest++) = 't';
			*pdest = ';';

		}
		else if(*psrc == '\'')
		{
			*(pdest++) = '&';
			*(pdest++) = 'a';
			*(pdest++) = 'p';
			*(pdest++) = 'o';
			*(pdest++) = 's';
			*pdest = ';';

		}
		else if(*psrc == '"')
		{
			*(pdest++) = '&';
			*(pdest++) = 'q';
			*(pdest++) = 'u';
			*(pdest++) = 'o';
			*(pdest++) = 't';
			*pdest = ';';

		}
		else
		{
			*pdest = *psrc;

		}

		*psrc ++;
		*pdest ++;

	}

	*pdest = 0;

	return result;

}

//TODO: ��� ��������
////-------------------------------------
//
//
//struct XmlAsMessage
//{
//};
//
//
//
////-------------------------------
//
//
//struct XmlAsParameters
//{
//};




#endif// INCLUDE__XML_H


