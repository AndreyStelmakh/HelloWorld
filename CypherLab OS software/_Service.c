
//--------------------------------------------------------------------------------------------------
/// Функции и классы для работы с сервисом
//
/// Создатель: Андрей Стельмах
/// 2012 год
//-----------------------------------------------------------------------------------------------


//ВАЖНО, не удаляй
//При линковке требуется увеличенный размер стека (от 0x002000) для работы функции Helper_GetFile.
//Смотри файл опций линкера (.lnk) в папке проекта


#ifndef INCLUDE__SRVPROXY_H
#define INCLUDE__SRVPROXY_H

#include "Config.h"
#include "8xTcpIp.h"
//#include "8300lib.h"
#include <stdlib.h>


#include "_Xml.h"
//#include "_Ctrls.h" //TODO: для ServiceHelper_errorHandler. Не правильно включать этот файл сюда, мне кажется
#include "_Macro.h"


char service_sockaddr_initialized = 0;
struct sockaddr_in service_sockaddr;


//SOCKET getConnectedSocket(SOCKET s)
SOCKET getConnectedSocket()
{
	SOCKET s = socket(PF_INET, SOCK_STREAM, 0);

	if(s >= 0)
	{
		DEBUG("сокет создан", 0)

		if(!service_sockaddr_initialized)
		{
			struct hostent *phostent;
			memset(&service_sockaddr, 0, sizeof(struct sockaddr_in));
			service_sockaddr.sin_family = AF_INET;
			service_sockaddr.sin_port = htons(SERVICE_PORT);
			phostent = gethostbyname(TargetHostName);

			memcpy(&service_sockaddr.sin_addr, phostent->h_addr_list[0], 4);

			service_sockaddr_initialized = 1;

		}

		//socket_push(s);

		{
			int r = connect(s, (struct sockaddr*)&service_sockaddr, sizeof(struct sockaddr_in));

			DEBUG("соединение", r);

			if(r != 0)
			{
				DEBUG("ОШИБКА\r\nНе удалось\r\nустановить\r\nсоединение\r\nс сервером", "Нажмите ESC");

				return -1;

			}

		}

		//socket_push(s);

		return s;

	}
	else
	{
		DEBUG("сокет не создан", 0);

		return -1;

	}

}

//похоже что не надо использовать в качестве команды и в качестве результата (первый и третий аргументы)
//один и тот же буфер,
//не знаю почему

//второй аргумент: вначале я заполнял результатом strlen, но, как оказалось, сервис определяет конец команды по завершающему нолику,
//а strlen этот нолик как раз таки не учитывает
int Service_ExecuteCommand(const char *commandText, unsigned int commandLength, char *result, unsigned int max_result_len, int *actual_result_len)
{
	SOCKET s = getConnectedSocket();

	//TODO: как-то указывать, что остались несчитанные данные

	if(s >= 0)
	{
		int result_length;
		int r;

		//это очень важная строка
		memset(result, 0, max_result_len);

		r = send(s, commandText, commandLength, 0);
		DEBUG("send", r);

		result_length = recv(s, result, max_result_len, 0);

		DEBUG("recv 1", result_length);

		if(actual_result_len != NULL)
		{
			*actual_result_len = result_length;

		}

		shutdown(s, 2);

		closesocket(s);

		return 0;

	}

	return -1;

}
int Service_ExecuteCommand_001(const char *commandText, unsigned int commandLength, char *result, unsigned int max_result_len, int *actual_result_len)
{
	SOCKET s = getConnectedSocket();

	//TODO: как-то указывать, что остались несчитанные данные

	if(s >= 0)
	{
		int result_length;
		int errorFlag;

		//это очень важная строка
		memset(result, 0, max_result_len);

		errorFlag = send(s, commandText, commandLength, 0);

		if(errorFlag != -1)
		{
	//		DEBUG("send", r);

			result_length = recv(s, result, max_result_len, 0);

			if(-1 != result_length)
			{
		//		DEBUG("recv 1", result_length);

				if(actual_result_len != NULL)
				{
					*actual_result_len = result_length;

				}

				errorFlag = 0;

			}
			else
			{
				errorFlag = -1;

			}

		}

		shutdown(s, 2);

		closesocket(s);

		return errorFlag;

	}

	return -1;

}

char ServiceSockaddrInitialized_2 = 0;
struct sockaddr_in ServiceSockaddr_2;

SOCKET getConnectedSocket_002()
{
	SOCKET s = socket(PF_INET, SOCK_STREAM, 0);

	if(s >= 0)
	{
		DEBUG("сокет создан", 0)

		if(!ServiceSockaddrInitialized_2)
		{
			struct hostent *phostent;
			memset(&ServiceSockaddr_2, 0, sizeof(struct sockaddr_in));
			ServiceSockaddr_2.sin_family = AF_INET;
			ServiceSockaddr_2.sin_port = htons(3000);
			phostent = gethostbyname(TargetHostName);

			memcpy(&ServiceSockaddr_2.sin_addr, phostent->h_addr_list[0], 4);

			ServiceSockaddrInitialized_2 = 1;

		}

		//socket_push(s);

		{
			int r = connect(s, (struct sockaddr*)&ServiceSockaddr_2, sizeof(struct sockaddr_in));

			DEBUG("соединение", r);

			if(r != 0)
			{
				DEBUG("ОШИБКА\r\nНе удалось\r\nустановить\r\nсоединение\r\nс сервером", "Нажмите ESC");

				return -1;

			}

		}

		//socket_push(s);

		return s;

	}
	else
	{
		DEBUG("сокет не создан", 0);

		return -1;

	}

}

int Service_ExecuteCommand_002(const char *commandText, unsigned int commandLength, char *resultBuffer, unsigned int resultBufferLength, unsigned long *pActualResponseLength, void (*pProgressFn)(unsigned int))
{
	SOCKET s = getConnectedSocket_002();
	int returnValue = -1;

	//TODO: как-то указывать, что остались несчитанные данные

	if(s >= 0)
	{
		//длина ответа без учета заголовка (сумма длин всех пакетов за вычетом 2*sizeof(unsigned long))
		unsigned long responseLength;
		//TODO: контрольная сумма пока не проверяется
		unsigned long responseChecksum;

		//ограничитель (на всякий случай) количества чтений (пакетов),
		//чтобы не застрять в этой функции, если приходят пакеты нулевой длины
		int maxPacketCount = 3000;

		//это очень важная строка
		memset(resultBuffer, 0, resultBufferLength);

		send(s, commandText, commandLength, 0);
		DEBUG("send", r);

		if(sizeof(responseLength) == recv(s, (char*)&responseLength, sizeof(responseLength), 0))
		{
			//проверяю: хватит ли места в предоставленном буфере для размещения всего ответа
			// +1 для нолика
			if(responseLength <= resultBufferLength + 1)
			{
				if(sizeof(responseChecksum) == recv(s, (char*)&responseChecksum, sizeof(responseChecksum), 0))
				{
					//сколько байт считано всего (по всем пакетам)
					unsigned int actualBytes = 0;
					int errorLoopExitFlag = 0;

					while(actualBytes < responseLength && errorLoopExitFlag == 0)
					{
			//			if(socket_hasdata(s))
						{
							//сколько байт считано за одно чтение
							int bytes = recv(s, resultBuffer + actualBytes, resultBufferLength - actualBytes, 0);

							if(bytes < 0)
							{
								DEBUG("ОШИБКА\r\nRecv вернула -1\r\nпри получении\r\nресурса", "Нажмите ESC")

								errorLoopExitFlag = 1;

							}
							else
							{
								/*if(pChecksum != NULL && bytes > 0)
								{
									checksumCounter = bytes;
									checksumPtr = (unsigned char*)bigBuffer;

									while(checksumCounter--)
									{
										*pChecksum += *(checksumPtr++);

									}

								}*/

								//append(fileDescriptor, bigBuffer, bytes);

								actualBytes += bytes;

								if(pProgressFn != NULL)
								{
									(*pProgressFn)((unsigned int)(100*actualBytes/responseLength));

								}

							}

						}

						if(maxPacketCount -- == 0)
						{
							errorLoopExitFlag = 1;

						}

					}

					if(pActualResponseLength != NULL)
					{
						*pActualResponseLength = responseLength;

					}

					resultBuffer[responseLength] = 0;

					returnValue = 0;

				}

			}

		}

		shutdown(s, 2);

		closesocket(s);

	}

	return returnValue;

}


//void ServiceHelper_GetFile(const char *ResourceUri, const char *fileToWriteTo, void (*pProgressFn)(unsigned int))
//{
//	int fileDescriptor;
//	SOCKET s;
//	int response_length;
//	char cmd[200];			//буфер для текста отправляемой серверу команды
//	char buffer[6000];		//промежуточный буфер в который будут записываться данные полученные по сети
//	long resource_length;	//длина файла, которую сообщит сервер
//
//	sprintf(cmd, "<cmd><cmdtype>non sql</cmdtype><cmdname>get file</cmdname></cmd><param><name>uri</name><value>%s</value></param>", ResourceUri);
//
////	puts("\r\nПеред Service_Exe");
//
//	if(Service_ExecuteCommand(cmd, strlen(cmd) + 1, buffer, sizeof(buffer), &response_length) == -1)
//	{
//		return;
//
//	}
//
//	resource_length = atol(buffer);
//
//	fileDescriptor = open(fileToWriteTo);
//	chsize(fileDescriptor, 0);
//
//	s = getConnectedSocket();
//
//	if(s >= 0)
//	{
//		//эмпирически установлено, что приходят куски размером не превышающие 6кБ
//		long actual_bytes;
//
//		////socket_push(s);
//
//		sprintf(cmd, "<cmd>Get resource</cmd><prm>%s</prm><prm>0</prm><prm>%ld</prm>", ResourceUri, resource_length);
//
//		send(s, cmd, strlen(cmd), 0);
//
//		actual_bytes = 0;
//
//		while(actual_bytes < resource_length)
//		{
////			if(socket_hasdata(s))
//			{
//				int bytes = 0;
//				bytes = recv(s, buffer, sizeof(buffer), 0);
//				if(bytes < 0)
//				{
//					DEBUG("ОШИБКА\r\nRecv вернула -1\r\nпри получении файла", "Нажмите ESC")
//
//				}
//				else
//				{
//					append(fileDescriptor, buffer, bytes);
//
//					actual_bytes += bytes;
//
//					if(pProgressFn != 0)
//					{
//						(*pProgressFn)((unsigned int)(100*actual_bytes/resource_length));
//
//					}
//
//				}
//
//			}
//
//		}
//
//		closesocket(s);
//
//	}
//
//	close(fileDescriptor);
//
//}
//
////как ни странно, но при вызове этого метода аргумент resourceSize уже известен
//void ServiceHelper_GetResourceById(long resourceId, long resourceSize, const char *fileToWriteTo, void (*pProgressFn)(unsigned int))
//{
//	int fileDescriptor;
//	SOCKET s;
//	int response_length;
//	char cmd[200];			//буфер для текста отправляемой серверу команды
//	char buffer[6000];		//промежуточный буфер в который будут записываться данные полученные по сети
//	//long resource_length;	//длина файла, которую сообщит сервер
//
////	sprintf(cmd, "<cmd>Get resource by id</cmd><prm>%ld</prm>", resourceId);
//
////	puts("\r\nПеред Service_Exe");
//
//	//if(ServiceHelper_ExecuteCommand(cmd, buffer, sizeof(buffer), &response_length) == -1)
//	//{
//	//	return;
//
//	//}
//
//	//resource_length = atol(buffer);
//
//	fileDescriptor = open(fileToWriteTo);
//	chsize(fileDescriptor, 0);
//
//	s = getConnectedSocket();
//
//	if(s >= 0)
//	{
//		//эмпирически установлено, что приходят куски размером не превышающие 6кБ
//		long actual_bytes;
//
//		////socket_push(s);
//
//		//TODO: может быть resourceSize-1 ?
//		sprintf(cmd, "<cmd>Get resource by id</cmd><prm>%ld</prm><prm>0</prm><prm>%ld</prm>", resourceId, resourceSize);
//
//		send(s, cmd, strlen(cmd), 0);
//
//		actual_bytes = 0;
//
//		while(actual_bytes < resourceSize)
//		{
////			if(socket_hasdata(s))
//			{
//				int bytes = 0;
//				bytes = recv(s, buffer, sizeof(buffer), 0);
//				if(bytes < 0)
//				{
//					DEBUG("ОШИБКА\r\nRecv вернула -1\r\nпри получении\r\nресурса", "Нажмите ESC")
//
//				}
//				else
//				{
//					append(fileDescriptor, buffer, bytes);
//
//					actual_bytes += bytes;
//
//					if(pProgressFn != NULL)
//					{
//						(*pProgressFn)((unsigned int)(100*actual_bytes/resourceSize));
//
//					}
//
//				}
//
//			}
//
//		}
//
//		closesocket(s);
//
//	}
//
//	close(fileDescriptor);
//
//}

//метод находящий в ответе сервиса параметр по заданному порядковому номеру
//лучше его не использовать
//используйте ServiceHelper_GetParam
const char* ServiceHelper_FindParam(const char* pResponse, int responseLength, unsigned int nParam)
{
	int i;
	int n = 0;	//счетчик параметров

	for(i = 0; i < responseLength; i++)
	{
		if(n == nParam)
		{
			return pResponse + i;

		}

		if(*(pResponse + i) == 0x1E)
		{
			n++;

		}

	}

	return NULL;

}

//метод получающий из ответа сервиса параметр по заданному порядковому номеру
char ServiceHelper_GetParam(const char* pResponse, int responseLength, unsigned int nParam, char *pVar, size_t varLength)
{
	const char *p = ServiceHelper_FindParam(pResponse, responseLength, nParam);

	if(p != NULL)
	{
		const char *end = strchr(p, 0x1E);
		size_t paramLength;

		//это очень важная строка (если параметр не будет найден
		memset(pVar, 0, varLength);

		if(end != NULL)
		{
			paramLength = end - p;

		}
		else
		{
			paramLength = strlen(p);

		}

		if(paramLength + 1 <= varLength)
		{
			strncpy(pVar, p, paramLength);

			//параметр добыт
			return 0;

		}

	}

	//параметр не добыт: не найден или не хватило места в pVar
	return -1;

}

char ServiceHelper_GetParamAsInt(const char* pResponse, int responseLength, unsigned int nParam, int *pValue)
{
	char valueBuffer[20];
	char *pValueStart;
	size_t valueLength;

	if(0 == ServiceHelper_GetParam(pResponse, responseLength, nParam, valueBuffer, sizeof(valueBuffer)))
	{
		*pValue = atoi(valueBuffer);

		return 0;

	}

	return -1;

}

char ServiceHelper_GetParamAsLong(const char* pResponse, int responseLength, unsigned int nParam, long *pValue)
{
	char valueBuffer[20];
	char *pValueStart;
	size_t valueLength;

	if(0 == ServiceHelper_GetParam(pResponse, responseLength, nParam, valueBuffer, sizeof(valueBuffer)))
	{
		*pValue = atol(valueBuffer);

		return 0;

	}

	return -1;

}

char ServiceHelper_GetParamAsDouble(const char* pResponse, int responseLength, unsigned int nParam, double *pValue)
{
	char valueBuffer[20];
	char *pValueStart;
	size_t valueLength;

	if(0 == ServiceHelper_GetParam(pResponse, responseLength, nParam, valueBuffer, sizeof(valueBuffer)))
	{
		*pValue = atof(valueBuffer);

		return 0;

	}

	return -1;

}

char ServiceHelper_handleError(const char* pResponse, int responseLength, int(*messageHandler)(long, char*))
{
	int nParam = 0;
	char responseParameter[1000];
	char xmlErrorBuffer[500];
	int errorPresense = 0;
//	const char *p;

//	while(NULL != (p = ServiceHelper_FindParam(pResponse, responseLength, nParam++)))
	while(0 == ServiceHelper_GetParam(pResponse, responseLength, nParam++, responseParameter, sizeof(responseParameter)))
	{
		int errorCount = 0;
		long errorCode = 0;
		char* pStartOfErrValue;
		size_t errValueLength;

		//кодом ошибки мы принимаем не код в err, а значение которое возвращает процедура в return (такая договорённость)
		XmlGetValueAsLong(responseParameter, responseParameter + sizeof(responseParameter), "return", 0, &errorCode);

		while(0 == XmlGetValueBoundaries(responseParameter, responseParameter + sizeof(responseParameter), "err", errorCount, &pStartOfErrValue, &errValueLength))
		//while(1 == Xml_getNValue(responseParameter, "err", errorCount, xmlErrorBuffer, sizeof(xmlErrorBuffer)))
		{
			//хотя бы одна ошибка обнаружена
			errorPresense = 1;

			if(messageHandler != NULL)
			{
				char errorMessage[400];
				long errorLevel;
				//unsigned long ul;
				memset(errorMessage, 0, sizeof(errorMessage));

				//Xml_getNValue(xmlErrorBuffer, "msg", 0, errorMessage, sizeof(errorMessage));
				XmlGetValue(pStartOfErrValue, pStartOfErrValue + errValueLength, "msg", 0, errorMessage, sizeof(errorMessage));

				if(0 != messageHandler(errorCode, errorMessage))
				{
					//messageHandler посчитал ошибку существенной, дальше обрабатывать не имеет смысла
					return 1;

				}
				else
				{
					//messageHandler посчитал ошибку несущественной, просматриваем ошибки дальше
					errorPresense = 0;

				}

				errorCount++;

			}
			else
			{
				//обработчик не задан, поэтому всё равно - сколько ошибок пришло,
				//возвращаю единичку, чтобы показать, что хотя бы одна ошибка была обнаружена
				return 1;

			}

		}

	}

	return errorPresense;

}

int Service_handleErrors(const char* pResponse, unsigned long responseLength, int(*errMessageHandler)(long, const char*))
{
	//int nParam = 0;
	//char responseParameter[1000];
	//char xmlErrorBuffer[500];
	int errorPresense = 0;

	int errorCount = 0;
	long errorCode = 0;
	char* pStartOfErrValue;
	size_t errValueLength;

	//кодом ошибки мы принимаем не код в err, а значение которое возвращает процедура в return (такая договорённость)
	XmlGetValueAsLong(pResponse, pResponse + sizeof(responseLength), "return", 0, &errorCode);

	while(0 == XmlGetValueBoundaries(pResponse, pResponse + responseLength, "err", errorCount, &pStartOfErrValue, &errValueLength))
	{
		//хотя бы одна ошибка обнаружена
		errorPresense = 1;

		if(errMessageHandler != NULL)
		{
			char errorMessage[400];
			memset(errorMessage, 0, sizeof(errorMessage));

			XmlGetValue(pStartOfErrValue, pStartOfErrValue + errValueLength, "msg", 0, errorMessage, sizeof(errorMessage));

			if(0 != errMessageHandler(errorCode, errorMessage))
			{
				//messageHandler посчитал ошибку существенной, дальше обрабатывать не имеет смысла
				return 1;

			}
			else
			{
				//messageHandler посчитал ошибку несущественной, просматриваем ошибки дальше
				errorPresense = 0;

			}

			errorCount++;

		}
		else
		{
			//обработчик не задан, поэтому всё равно - сколько ошибок пришло,
			//возвращаю единичку, чтобы показать, что хотя бы одна ошибка была обнаружена
			return 1;

		}

	}

	return errorPresense;

}


int ServiceHelper_GetResourceById(long resourceId, const char *fileToWriteTo, void (*pProgressFn)(unsigned int))
{
	SOCKET s = getConnectedSocket_002();
	//TODO: ограничитель (на всякий случай) количества чтений (пакетов),
	//чтобы не застрять в этой функции, если приходят пакеты нулевой длины
	int maxPacketCount = 3000;

	int errorExitFlag = 0;

	//TODO: как-то указывать, что остались несчитанные данные

	if(s >= 0)
	{
		//длина ответа без учета заголовка (сумма длин всех пакетов за вычетом 2*sizeof(unsigned long))
		unsigned long responseLength;
		//TODO: контрольная сумма пока не проверяется
		unsigned long responseChecksum;

		//TODO: ограничитель (на всякий случай) количества чтений (пакетов),
		//чтобы не застрять в этой функции, если приходят пакеты нулевой длины
		int maxPacketCount = 3000;

		{
			char commandText[200];
			sprintf(commandText, "<cmd type='service' name='get resource'><resourceid>%ld</resourceid></cmd>", resourceId);

			send(s, commandText, strlen(commandText), 0);

		}

		if(sizeof(responseLength) == recv(s, (char*)&responseLength, sizeof(responseLength), 0))
		{
			if(sizeof(responseChecksum) == recv(s, (char*)&responseChecksum, sizeof(responseChecksum), 0))
			{
				//сколько байт считано всего (по всем пакетам)
				unsigned long actualBytes = 0;

				//флаг, нужен чтобы правильно освободить ресурсы (сокет там закрыть и типа того)
				int errorLoopExitFlag = 0;
				int fileDescriptor = open(fileToWriteTo);

				char bigBuffer[6000];

				chsize(fileDescriptor, 0);

				while(actualBytes < responseLength && errorExitFlag == 0)
				{
		//			if(socket_hasdata(s))
					{
						//сколько байт считано за одно чтение
						int bytes = recv(s, bigBuffer, sizeof(bigBuffer), 0);

						if(bytes < 0)
						{
							DEBUG("ОШИБКА\r\nRecv вернула -1\r\nпри получении\r\nресурса", "Нажмите ESC")

							errorExitFlag = 1;

						}
						else
						{
							/*if(pChecksum != NULL && bytes > 0)
							{
								checksumCounter = bytes;
								checksumPtr = (unsigned char*)bigBuffer;

								while(checksumCounter--)
								{
									*pChecksum += *(checksumPtr++);

								}

							}*/

							append(fileDescriptor, bigBuffer, bytes);

							actualBytes += bytes;

							if(pProgressFn != NULL)
							{
								(*pProgressFn)((unsigned int)(100*actualBytes/responseLength));

							}

						}

					}

					if(maxPacketCount -- == 0)
					{
						errorExitFlag = 1;

					}

				}

				close(fileDescriptor);

			}
			else
			{
				errorExitFlag = 1;

			}

		}
		else
		{
			errorExitFlag = 1;

		}

		shutdown(s, 2);

		closesocket(s);

	}

	if(errorExitFlag == 0)
	{
		return 0;

	}
	else
	{
		remove(fileToWriteTo);

		return -1;

	}

}



#endif	//INCLUDE__SRVPROXY_H


