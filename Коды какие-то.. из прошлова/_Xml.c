
//--------------------------------------------------------------------------------------------------
/// Функции и классы для работы с данными XML формата
//
/// Создатель: Андрей Стельмах
/// 2012 год
//-----------------------------------------------------------------------------------------------


#ifndef INCLUDE__XML_H
#define INCLUDE__XML_H

#include <string.h>
#include <stdlib.h>

#include "_Macro.h"

#include "_Xml.h"


//находит в тексте (xml) заданный тег (tag) и копирует его содержимое в указанный буфер
//возвращает 1, если получилось
//TODO: устаревшая, исключать
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
			//ищем следующее вхождение данного тега
			pBegin = (char*)strstr(pBegin, begin_tag);

		}
		else
		{
			pEnd = (char*)strstr(pBegin, end_tag);

			if(pEnd != 0)
			{
				size_t value_length = pEnd - pBegin;

				if(value_length + 1 > buffer_length) // +1 для завершающего нолика
				{
					// буфер имеет маловатый размер >:(
					return 0;

				}

				strncpy(buffer, pBegin, value_length);

				buffer[value_length] = 0;

				return 1;

			}
			else
			{
				DEBUG("конец xml не найден", 0);

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
		//начало выставляю на отметку "сразу после окончания стартового тега"
		pBegin += strlen(beginTag);

		if(n-- != 0)
		{
			//ищем следующее вхождение стартового тега
			pBegin = (char*)strstr(pBegin, beginTag);

		}
		else
		{
			pEnd = (char*)strstr(pBegin, endTag);

			//если не дошли до завершающего нолика и не вышли за правую границу исходной области xml
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
		//ShowMessage(valueBuffer, "Нажмите Enter");

		*pValue = atol(valueBuffer);

		//sprintf(valueBuffer, "%ld", *pValue);

		//ShowMessage(valueBuffer, "Нажмите Enter (2)");

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

	//выставляю на первую строку таблицы
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
			NULL// а длина в данной ситуации не интересна
		);

}

int XmlAsTable_MoveNext(struct XmlAsTable* pXmlAsTable)
{
	//тут немного нерационально, но работать будет
	return
		XmlGetValueBoundaries
		(
			pXmlAsTable->pCurrentRowValue,
			pXmlAsTable->pRecordsetEnd,
			pXmlAsTable->RowTag,
			0,
			&pXmlAsTable->pCurrentRowValue,
			NULL// а длина в данной ситуации не интересна
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

//заменяет & на &amp;  < на &lt;  > на &gt;  " на &quot;  ' на &apos;
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

//TODO: под вопросом
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


