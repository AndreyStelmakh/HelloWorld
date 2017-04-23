
//--------------------------------------------------------------------------------------------------
/// Функции и классы стандартных элементов управления графического интерфейса терминала
//
/// Создатель: Андрей Стельмах
/// 2012 год
//-----------------------------------------------------------------------------------------------

#include "header.h"
//#include <math.h> 


#include "_Ctrls.h"
#include "_Macro.h"
#include "_Timer.h"

void Toolbar_Redraw(struct Toolbar *);
void TextBox_Redraw(struct TextBox *);
void VList_Redraw(struct VirtualList *);

char* pInputBuf = 0;

//классная функция которая объединяет потокb символов от сканера и от клавиатуры
//в один общий поток (П)
//работает наподобие getchar только не блокирует,
//если потоки пусты возвращает 0
//кроме того, функция поджигает экранчик при нажатиях кнопок и сканировании
char GetInput(char *pIsBarcodeScanned)
{
	if(pIsBarcodeScanned != NULL)
	{
		*pIsBarcodeScanned = 0;

	}

	//возвращаю очередной символ из потока сканера
	//если в потоке есть символы
	if(pInputBuf != NULL)
	{
		if(*pInputBuf != 0)
		{
			return *(pInputBuf++);

		}

		pInputBuf = NULL;

	}

	//проверяю не готов ли сканер заполнить свой поток новыми символами
	//если да, то позиционирую курсор на его начало, возвращаю символ и переставляю указатель на следующий символ этого потока
	if(Decode() != 0)
	{
		if(pIsBarcodeScanned != NULL)
		{
			*pIsBarcodeScanned = 1;

		}

		pInputBuf = CodeBuf;
		//*ppBarcode = CodeBuf;

		lcd_backlit(1);

		return *(pInputBuf++);

	}
	//проверяю клавиатурный поток на наличие символов
	else if(kbhit() != 0/* && pKey != NULL*/)
	{
		char key = getchar();

		lcd_backlit(1);

		return key;

	}

	//если не было символов ни в сканерном потоке ни в клавиатурном, то возвращаю 0
	return 0;

}

char Input_Get(char *pIsDataInBarcodeBuffer)
{
	if(pIsDataInBarcodeBuffer != NULL)
	{
		*pIsDataInBarcodeBuffer = 0;

	}

	//возвращаю очередной символ из потока сканера
	//если в потоке есть символы
	if(pInputBuf != NULL)
	{
		pInputBuf ++;

		if(*pInputBuf != 0)
		{
			if(pIsDataInBarcodeBuffer != NULL)
			{
				*pIsDataInBarcodeBuffer = 1;

			}

			return *pInputBuf;

		}

		pInputBuf = NULL;

	}

	//проверяю не готов ли сканер заполнить свой поток новыми символами
	//если да, то позиционирую курсор на его начало, возвращаю символ и переставляю указатель на следующий символ этого потока
	if(Decode() != 0)
	{
		pInputBuf = CodeBuf;

		if(pIsDataInBarcodeBuffer != NULL)
		{
			*pIsDataInBarcodeBuffer = 1;

		}

		lcd_backlit(1);

		return *pInputBuf;

	}
	//проверяю клавиатурный поток на наличие символов
	else if(kbhit() != 0/* && pKey != NULL*/)
	{
		char key = getchar();

		lcd_backlit(1);

		return key;

	}

	//если не было символов ни в сканерном потоке ни в клавиатурном, то возвращаю 0
	return 0;

}

void ClearInput()
{
	clr_kb();

	pInputBuf = NULL;

}

void Object_Constructor(struct Object *object)
{
	//object->Used = 1;
	//object->NeedsForceDelete = 0;
	object->Type = CTRL_OBJECT;

}

void CtrlBase_Constructor(struct CtrlBase *ctrl)
{
	Object_Constructor((struct Object*)ctrl);

	ctrl->Base.Type = CTRL_CTRLBASE;

	ctrl->Bounds.Height = 1;
	ctrl->Bounds.Width = 20;
	ctrl->Bounds.X = 0;
	ctrl->Bounds.Y = 0;

//	ctrl->IsVisible = 1;

}

//"виртуальные" методы:
void redraw_ctrl(struct CtrlBase* ctrl)
{
	switch(ctrl->Base.Type)
	{
	case CTRL_TEXTBOX:
		TextBox_Redraw((struct TextBox*)ctrl);
		break;

	case CTRL_VIRTUALMENU:
	case CTRL_MENU:
		VList_Redraw((struct VirtualList*)ctrl);
		break;

	case CTRL_TOOLBAR:
		Toolbar_Redraw((struct Toolbar*)ctrl);
		break;

	}

}

int putkey_ctrl(struct CtrlBase* ctrl, char key)
{
	switch(ctrl->Base.Type)
	{
	case CTRL_TEXTBOX:
		return TextBox_PutKey((struct TextBox*)ctrl, key);

	case CTRL_VIRTUALMENU:
	case CTRL_MENU:
		return VList_PutKey((struct VirtualList*)ctrl, key);

	default:
		return 0;//клавиша не обработана
	}

}


void Toolbar_Constructor(struct Toolbar *bar)
{
	memset(bar, 0, sizeof(struct Toolbar));

	CtrlBase_Constructor((struct CtrlBase*)bar);

	bar->Base.Base.Type = CTRL_TOOLBAR;

	bar->ArrowsLR_Yoffset = 27;
	bar->ArrowUp_Yoffset = 34;
	bar->ArrowDown_Yoffset = 60;

}

void Toolbar_Redraw(struct Toolbar *bar)
{
	SetVideoMode(VIDEO_NORMAL);

	//двигаю батарейку вверх
	//стираю артефакты
	{
		char BATTERY_ERASER[] = {0,0,0,0};
		BC_Y = 47;
		show_image(120, 58, 8, 2, BATTERY_ERASER);

	}

	{
		if(bar->ArrowLeft != 0 && bar->ArrowRight != 0)
		{
			char ARROWS[] = {20, 54, 119, 54, 20};

			show_image(121, bar->ArrowsLR_Yoffset, 8, 5, ARROWS);

		}
		else if(bar->ArrowLeft != 0)
		{
			char ARROW[] = {20&7, 54&7, 119&7, 54&7, 20&7};

			show_image(121, bar->ArrowsLR_Yoffset, 8, 5, ARROW);

		}
		else if(bar->ArrowRight != 0)
		{
			char ARROW[] = {20&112, 54&112, 119&112, 54&112, 20&112};

			show_image(121, bar->ArrowsLR_Yoffset, 8, 5, ARROW);

		}
		else
		{
			char ARROW_OFF[] = {0, 0, 0, 0, 0};

			show_image(121, bar->ArrowsLR_Yoffset, 8, 5, ARROW_OFF);

		}

	}

	if(bar->ArrowUp != 0)
	{
		char ARROW[] = {16, 56, 124, 254};

		show_image(120, bar->ArrowUp_Yoffset, 8, 4, ARROW);

	}
	else
	{
		char ARROW_OFF[] = {0, 0, 0, 0};

		show_image(120, bar->ArrowUp_Yoffset, 8, 4, ARROW_OFF);

	}

	if(bar->ArrowDown != 0)
	{
		char ARROW[] = {254, 124, 56, 16};

		show_image(120, bar->ArrowDown_Yoffset, 8, 4, ARROW);

	}
	else
	{
		char ARROW_OFF[] = {0, 0, 0, 0};

		show_image(120, bar->ArrowDown_Yoffset, 8, 4, ARROW_OFF);

	}

	//if(bar->LowBattery != 0)
	//{
	//	char BATTERY[] = {28, 54, 35, 38, 42, 50, 98, 62};

	//	show_image(120, 50, 8, 8, BATTERY);

	//}
	//else
	//{
	//	char BATTERY[] = {0,0,0,0,0,0,0,0};

	//	show_image(120, 50, 8, 8, BATTERY);

	//}


}

//---------------------------

void TextBox_Redraw(struct TextBox *ctrl)
{
	char buf[50];
	int x, y;

	//wherexy(&x, &y);

	x /*+*/= ctrl->Base.Bounds.X;
	y /*+*/= ctrl->Base.Bounds.Y;

	if(((struct CtrlBase*)ctrl)->IsSelected)
	{
		SetVideoMode(VIDEO_REVERSE);

	}
	else
	{
		SetVideoMode(VIDEO_NORMAL);

	}

	//сперва очищаю
	memset(buf, ' ', sizeof(buf));
	buf[ctrl->Base.Bounds.Width] = 0;
	gotoxy(x, y);
	puts(buf);

	//теперь вывожу по чистому
	strncpy(buf, ctrl->pValue, ctrl->Base.Bounds.Width);
	buf[ctrl->Base.Bounds.Width] = 0;
	if(ctrl->IsRightAligned == 0)
	{
		gotoxy(x, y);

	}
	else
	{
		gotoxy(x + ctrl->Base.Bounds.Width - strlen(ctrl->pValue), y);

	}

	puts(buf);

	//if(ctrl->IsEdited == 1)
	//{
	//	//TODO: нарисовать курсор (например подчеркнуть контрол)

	//}

	SetVideoMode(VIDEO_NORMAL);

}

void TextBox_Constructor(struct TextBox *ctrl, char* textBuffer)
{
	memset(ctrl, 0, sizeof(struct TextBox));

	CtrlBase_Constructor((struct CtrlBase*)ctrl);

	ctrl->Base.Base.Type = CTRL_TEXTBOX;

	ctrl->pValue = textBuffer;

	//ctrl->Base.Height = 1;
	//ctrl->Base.Width = 5;

}

int InputFilter_Int(struct TextBox *textBox, char character)
{
	dis_alpha();

	//допустимы только цифры
	if
	(	(0x30 <= character && character <= 0x39)
		|| KEY_BS == character
	)
	{
		return 1;

	}

	return 0;

}
//четыре знака после десятичной точки
int InputFilter_Num4(struct TextBox *textBox, char character)
{
	char* pDecimalPoint = strchr(textBox->pValue, 0x2E);

	//если десятичной точки еще нет, то можно
	if(pDecimalPoint == NULL && character == 0x2E)
	{
		return 1;

	}

	if(character == KEY_BS)
	{
		return 1;

	}

	if(0x30 <= character && character <= 0x39)
	{
		//определяю количество знаков после десятичной точки (если четыре, то досвидания)
		if(pDecimalPoint == NULL)
		{
			return 1;

		}

		if(strlen(pDecimalPoint) <= 4 )
		{
			return 1;

		}

	}

	return 0;

}
//все печатные символы
int InputFilter_NoLimits(struct TextBox *textBox, char character)
{
	if
	(	(0x20 <= character && character <= 0x89)
		|| KEY_BS == character
	)
	{
		return 1;

	}

	return 0;

}
//без редактирования
int InputFilter_ReadOnly(struct TextBox *textBox, char character)
{
	return 0;

}

//TODO: упразднить TextBox_..: _BeginEdit, _AddSymbol, _Backspace
int TextBox_PutKey(struct TextBox *ctrl, char key)
{
	if(ctrl->inputFilterFunc == NULL)
	{
		if(InputFilter_ReadOnly(ctrl, key) == 0)
		{
			return 0;//клавиша не обработана

		}

	}
	else if(ctrl->inputFilterFunc(ctrl, key) == 0)
	{
		return 0;//клавиша не обработана

	}


	if(key == KEY_BS)
	{
		if(strlen(ctrl->pValue) > 0)
		{
			*(ctrl->pValue + strlen(ctrl->pValue) - 1) = 0;

			TextBox_Redraw(ctrl);

		}

		return 1;//клавиша обработана

	}
	else
	{
		//TextBox_AddSymbol(ctrl, key);
		if(ctrl->Base.Bounds.Width > strlen(ctrl->pValue))
		{
			char asChar[2];
			asChar[0] = key;//(char)key;
			asChar[1] = 0;

			strcat(ctrl->pValue, asChar);

			TextBox_Redraw(ctrl);

		}

		return 1;//клавиша обработана
	}

}

int TextBox_IsValueValid(struct TextBox *ctrl)
{
	if(ctrl->resultValidationFunc != NULL)
	{
		return ctrl->resultValidationFunc(ctrl->pValue);

	}

	return 1;

}

void ProgressBar_Constructor(struct ProgressBar *bar)
{
	CtrlBase_Constructor((struct CtrlBase*)bar);

}

void ProgressBar_SetValue(struct ProgressBar *bar, unsigned int value)
{
	if(value <= 100)	//100 процентов
	{
		if(bar->Value > value)
		{
			rectangle(bar->Base.Bounds.X, bar->Base.Bounds.Y, bar->Base.Bounds.X + bar->Base.Bounds.Width, bar->Base.Bounds.Y + bar->Base.Bounds.Height, SHAPE_FILL, DOT_CLEAR);

	//		rectangle(ctrl->X, ctrl->Y, (ctrl->X + ctrl->Width)*ctrl->Value/100, ctrl->Y + ctrl->Height, SHAPE_FILL, DOT_CLEAR);

		}

		bar->Value = value;

		rectangle(bar->Base.Bounds.X, bar->Base.Bounds.Y, bar->Base.Bounds.X + bar->Base.Bounds.Width, bar->Base.Bounds.Y + bar->Base.Bounds.Height, SHAPE_NORMAL, DOT_MARK);

		rectangle(bar->Base.Bounds.X, bar->Base.Bounds.Y, (bar->Base.Bounds.X + bar->Base.Bounds.Width)*bar->Value/100, bar->Base.Bounds.Y + bar->Base.Bounds.Height, SHAPE_FILL, DOT_MARK);

	}

}



//---------------------------------------------- виртуальное меню

// функция преобразует координату строки (индекс) к действительным значениям
// предположим есть список из пяти элементов с индексами от 0 до 4 (всего пять),
// у которого запросили элемент с индексом 6, в циклическом меню индексу 6 соответствует элемент 1
// запрашиваемый элемент: 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 ..
// получаемый элемент   : 0 1 2 3 4 0 1 2 3 4  0  1  2  3  4  0 ..
unsigned int VList_Normalize(struct VirtualList* menu, int index)
{
	if(index >= 0)
	{
		return ((index % menu->ItemsCount) + menu->ItemsCount) % menu->ItemsCount;
	}
	else
	{
		return (-((-index) % menu->ItemsCount) + menu->ItemsCount) % menu->ItemsCount;

	}

}

// правит неверные (если такие есть) значения отвечающие за положения курсора
void VList_Correct(struct VirtualList *menu)
{
	int i;

	if(menu->CursorLowestPosition >= menu->Base.Bounds.Height)
	{
		menu->CursorLowestPosition = menu->Base.Bounds.Height - 1;

	}

	if(menu->CursorHighestPosition >= menu->Base.Bounds.Height)
	{
		menu->CursorHighestPosition = menu->Base.Bounds.Height - 1;

	}

	if(menu->CursorHighestPosition >= menu->ItemsCount)
	{
		menu->CursorHighestPosition = menu->ItemsCount - 1;

	}


	if(menu->CursorHighestPosition < menu->CursorLowestPosition)
	{
		i = menu->CursorHighestPosition;
		menu->CursorHighestPosition = menu->CursorLowestPosition;
		menu->CursorLowestPosition = i;

	}

	//if(menu->CurrentItem < menu->TopItem + menu->CursorLowestPosition)// || menu->CursorHighestPosition < menu->CurrentItem)
	//{
	//	menu->CurrentItem = menu->TopItem + menu->CursorLowestPosition;

	//}

	//if(menu->TopItem + menu->CursorHighestPosition < menu->CurrentItem)
	//{
	//	menu->CurrentItem = menu->TopItem + menu->CursorHighestPosition;

	//}

}

void VList_Constructor(struct VirtualList *menu)
{
	memset(menu, 0, sizeof(struct VirtualList));

	CtrlBase_Constructor((struct CtrlBase*)menu);

	menu->Base.Base.Type = CTRL_VIRTUALMENU;

	menu->ItemsCount = 0;

	menu->Base.Bounds.Height = 8;
	menu->AllowCyclic = 1;
	menu->pToolbar = NULL;
	menu->AllowScrollMarkers = 1;
	menu->EnableCursor = 1;

//	menu->IsInitialized = 1;

	VList_Correct(menu);

}

void VList_Clear(struct VirtualList *menu)
{
	menu->CurrentItem = 0;
	menu->ItemsCount = 0;
	menu->TopItem = 0;

}

int VList_IsCyclic(struct VirtualList *menu)
{
	//TODO: определить - длиннее ли меню экрана (7строк из них одна двойной высоты, итого - 8)
	if(menu->ItemsCount >= menu->Base.Bounds.Height && menu->AllowCyclic)
	{
		return 1;

	}
	else
	{
		return 0;

	}

}

int VList_HasRowsOverTheTop(struct VirtualList *menu)
{
	if(menu->TopItem > 0)
	{
		return 1;

	}
	else
	{
		return 0;

	}

}

int VList_HasRowsUnderTheBottom(struct VirtualList *menu)
{
	if(menu->TopItem + menu->Base.Bounds.Height < menu->ItemsCount)
	{
		return 1;

	}
	else
	{
		return 0;

	}
}

void VList_Redraw(struct VirtualList *menu)
{
	int i;
	char spaces[30];

	memset(spaces, ' ', sizeof(spaces));
	spaces[menu->Base.Bounds.Width] = 0;

	for(i = 0; i < menu->Base.Bounds.Height; i ++)
	{
		unsigned int itemN = VList_Normalize(menu, menu->TopItem + i);

		SetFont(FONT_6X8);
		SetVideoMode(VIDEO_NORMAL);
		// очищаю фон
		gotoxy(menu->Base.Bounds.X, menu->Base.Bounds.Y + i);
		puts(spaces);

		if(itemN != -1)
		{
			struct CtrlBase *item = menu->OnGetMemberData(menu, itemN);

			if(item != NULL)
			{
				VList_GetItemBounds(menu, itemN, &item->Bounds);

				if(itemN == menu->CurrentItem && menu->EnableCursor)
				{
					item->IsSelected = 1;//SetVideoMode(VIDEO_REVERSE);

				}
				else
				{
					item->IsSelected = 0;//SetVideoMode(VIDEO_NORMAL);

				}

				redraw_ctrl(item);

			}

		}

	}

	if(menu->pToolbar != NULL)
	{
		if(VList_IsCyclic(menu) || VList_HasRowsOverTheTop(menu))
		{
			menu->pToolbar->ArrowUp = 1;

		}
		else
		{
			menu->pToolbar->ArrowUp = 0;

		}

		if(VList_IsCyclic(menu) || VList_HasRowsUnderTheBottom(menu))
		{
			menu->pToolbar->ArrowDown = 1;

		}
		else
		{
			menu->pToolbar->ArrowDown = 0;

		}

		Toolbar_Redraw(menu->pToolbar);

	}
	else
	{
		if(menu->AllowScrollMarkers)
		{
			int LETTERH = 8;
			int LETTERW = 6;
			int xr = (((int)menu->Base.Bounds.X + (int)menu->Base.Bounds.Width - 1)*LETTERW) + 5;
			int yt = menu->Base.Bounds.Y * LETTERH;
			int yb = ((menu->Base.Bounds.Y + menu->Base.Bounds.Height) * LETTERH) - 1;

			// если меню циклическое или если есть строки над верхней границей, то включаю верхний маркер
			if(VList_IsCyclic(menu) || VList_HasRowsOverTheTop(menu))
			{
				line
				(
					xr - 4,	yt,
					xr, yt,
					DOT_MARK
				);

				line
				(
					xr, yt,
					xr, yt + 1,
					DOT_MARK
				);

			}

			// если меню циклическое или если под нижней границей меню есть еще строки, то включаю верхний маркер
			if(VList_IsCyclic(menu) || VList_HasRowsUnderTheBottom(menu))
			{
				line
				(
					xr - 4,	yb,
					xr, yb,
					DOT_MARK
				);

				line
				(
					xr, yb,
					xr, yb - 1,
					DOT_MARK
				);

			}

		}

	}

}

void VList_MovePrevious(struct VirtualList *menu)
{
	VList_Correct(menu);

	//если курсор есть, и его положение не достигло крайней верхней точки (menu->CursorLowestPosition),
	//то не кручу меню, а только двигаю курсор
	if
	(	menu->EnableCursor
		&& (VList_Normalize(menu, (int)menu->CurrentItem - (int)menu->TopItem) > menu->CursorLowestPosition)
	)
	{
		menu->CurrentItem = VList_Normalize(menu, ((int)menu->CurrentItem) - 1);

	}
	else if
	(
		//если курсора нет, то кручу меню до границы экрана
		(menu->EnableCursor == 0 && menu->TopItem > 0)
		//если курсор есть, то кручу меню пока курсор не встанет на первый (нулевой) элемент
		||(menu->EnableCursor != 0 && (0 < menu->CurrentItem))
		//если меню циклическое, то крутить меню можно безусловно
		|| VList_IsCyclic(menu) == 1
	)
	{
		menu->TopItem = VList_Normalize(menu, ((int)menu->TopItem) - 1);
		menu->CurrentItem = VList_Normalize(menu, ((int)menu->CurrentItem) - 1);

	}

	VList_Redraw(menu);

}

void VList_MoveNext(struct VirtualList *menu)
{
	VList_Correct(menu);

	//если курсор есть, и его положение не достигло крайней нижней точки (menu->CursorHighestPosition),
	//то не кручу меню, а только двигаю курсор
	if
	(	menu->EnableCursor
		&& ( menu->CursorHighestPosition > VList_Normalize(menu, (int)menu->CurrentItem - (int)menu->TopItem))
	)
	{
		menu->CurrentItem = VList_Normalize(menu, ((int)menu->CurrentItem) + 1);

	}
	else if
	(
		//если курсора нет, то кручу до границы экрана
		(menu->EnableCursor == 0 && (menu->TopItem + menu->Base.Bounds.Height < menu->ItemsCount))
		//если курсор есть, то кручу пока курсор не встанет на последний элемент
		||(menu->EnableCursor != 0 && (menu->CurrentItem + 1 < menu->ItemsCount))
		//если меню циклическое, то крутить можно безусловно
		|| VList_IsCyclic(menu) == 1
	)
	{
		menu->TopItem = VList_Normalize(menu, ((int)menu->TopItem) + 1);
		menu->CurrentItem = VList_Normalize(menu, ((int)menu->CurrentItem) + 1);

	}

	VList_Redraw(menu);

}

void VList_GetItemBounds(struct VirtualList *menu, unsigned int nItem, struct Rectangle* bounds)
{
	bounds->X = menu->Base.Bounds.X;
	bounds->Width = menu->Base.Bounds.Width;

	bounds->Height = 1;

	bounds->Y = menu->Base.Bounds.Y + VList_Normalize(menu, (int)nItem - (int)menu->TopItem);

}

int VList_PutKey(struct VirtualList *menu, char key)
{
	if(key == KEY_UP)//вверх
	{
		VList_MovePrevious(menu);

		return 1;

	}
	else if(key == KEY_DOWN)//вниз
	{
		VList_MoveNext(menu);

		return 1;

	}
	else
	{
		struct CtrlBase* item = menu->OnGetMemberData(menu, menu->CurrentItem);

		if(item != NULL)
		{
			VList_GetItemBounds(menu, menu->CurrentItem, &item->Bounds);

			//TODO: здесь это зачем-то нужно? уточнить
			item->IsSelected = 1;

			return putkey_ctrl(item, key);

		}

		return 0;

	}

}

struct CtrlBase* Menu_OnGetMemberData(struct List* menu, unsigned int nItem)
{
	if(nItem < menu->Base.ItemsCount)
	{
		menu->_buffer.pValue = menu->Items[nItem];

		menu->_buffer.inputFilterFunc = InputFilter_ReadOnly;

		return (struct CtrlBase*)&menu->_buffer;

	}

	return NULL;

}

void List_Constructor(struct List* ctrl)
{
	VList_Constructor(&ctrl->Base);
	TextBox_Constructor(&ctrl->_buffer, NULL);

	ctrl->Base.Base.Base.Type = CTRL_MENU;

	ctrl->Base.OnGetMemberData = (struct CtrlBase*(*)(struct VirtualList*, unsigned int))Menu_OnGetMemberData;

}

void Menu_AppendText(struct List *menu, const char* str)
{
	unsigned int row;
	unsigned int str_len;

	row = 0;
	str_len = strlen(str);

	//пока суммарная длина добавленных строк меню меньше длины текста, повторяем
	while(row*menu->Base.Base.Bounds.Width < str_len)
	{
		if(menu->Base.ItemsCount >= MENU_MAX_SIZE)
		{
			return;

		}

		strncpy(menu->Items[menu->Base.ItemsCount++], str + row*menu->Base.Base.Bounds.Width, menu->Base.Base.Bounds.Width);

		row ++;

	}

}

void Caption_Redraw(unsigned int x, unsigned int y, const char *captionText)
{
	char BULLET[] = {0, 0, 0, 30, 30, 30, 0, 0};

	show_image(x * 6, y * 8, 6, 8, BULLET);

	gotoxy(x+1, y);

	puts(captionText);

}


//--------------------------------------

//unsigned char g_ScreenShot[128*64/8];//размеры экранчика в пикселях разделить на 8 (в байт влазит 8 пикселей)

int m(void* pArgument)
{
	struct RunningLine *runningLine = (struct RunningLine *)pArgument;

	RLine_Tick(runningLine);

	return 0;

}

//Диалог ОК\ESC
char ShowMessage(const char *message, const char *description)
{
	struct RunningLine descriptionRLine;
	unsigned char screenShot[128*64/8];//размеры экранчика в пикселях разделить на 8 (в байт влазит 8 пикселей)

	RLine_Constructor(&descriptionRLine);
	descriptionRLine.pString = description;
	descriptionRLine.Base.Bounds.Y = 7;

	get_image(0, 0, 128, 64, screenShot);

	{
		int prevVideoMode = GetVideoMode();
		int prevFont = GetFont();

		SetVideoMode(VIDEO_NORMAL);
		SetFont(FONT_6X8);

		clr_scr();

		puts(message);

		//если description длиннее ширины экрана, то будем эту строку прокручивать
		if(strlen(description) > 20)
		{
			Timer_Subscribe(m, &descriptionRLine);

		}
		else
		{
			gotoxy(0, 7);
			puts(description);

		}

		while(1)
		{
			char key = GetInput(NULL);

			if(key == KEY_ENTER || key == KEY_ESC)
			{
				Timer_Unsubscribe(m);

				clr_scr();

				show_image(0, 0, 128, 64, screenShot);

				SetVideoMode(prevVideoMode);
				SetFont(prevFont);

				if(key == KEY_ENTER)
				{
					return 0;

				}
				else
				{
					return -1;

				}

			}

		}

	}

}

//Тот же диалог, но выход по любой кнопке
void ShowMessageAnyKey(const char *message, const char *description)
{
	struct RunningLine descriptionRLine;
	unsigned char screenShot[128*64/8];//размеры экранчика в пикселях разделить на 8 (в байт влазит 8 пикселей)

	RLine_Constructor(&descriptionRLine);
	descriptionRLine.pString = description;
	descriptionRLine.Base.Bounds.Y = 7;

	get_image(0, 0, 128, 64, screenShot);

	{
		int prevVideoMode = GetVideoMode();
		int prevFont = GetFont();

		SetVideoMode(VIDEO_NORMAL);
		SetFont(FONT_6X8);

		clr_scr();

		puts(message);

		//если description длиннее ширины экрана, то будем эту строку прокручивать
		if(strlen(description) > 20)
		{
			Timer_Subscribe(m, &descriptionRLine);

		}
		else
		{
			gotoxy(0, 7);
			puts(description);

		}

		while(1)
		{
			char isBarcodeScanned;
			char key = GetInput(&isBarcodeScanned);

			if(key != 0 || isBarcodeScanned == 1)
			{
				Timer_Unsubscribe(m);

				clr_scr();

				show_image(0, 0, 128, 64, screenShot);

				SetVideoMode(prevVideoMode);
				SetFont(prevFont);

				return;

			}

		}

	}

}

//--------------------------------------
void RLine_Constructor(struct RunningLine *pCtrl)
{
	memset(pCtrl, 0, sizeof(struct RunningLine));

	CtrlBase_Constructor((struct CtrlBase*)pCtrl);

	pCtrl->Base.Base.Type = CTRL_RUNNINGLINE;

	pCtrl->_Step = 1;
	pCtrl->TicksAtStringBeginning = 0;
	//pCtrl->TicksBeforeStart = 0;

}
void RLine_Tick(struct RunningLine *pCtrl)
{
	char endingSpacesNumber = 3;
	char buffer[25];
	int x, y;

	//TODO: всегда проверять чтобы длина буфера была больше значения параметра ширины

	if(pCtrl->_pCurrentPosition == 0)
	{
		pCtrl->_pCurrentPosition = pCtrl->pString;

	}

	if(pCtrl->pString == 0)
	{
		return;

	}

	////выдерживаю паузу на старте
	//if(pCtrl->TicksBeforeStart != 0)
	//{
	//	pCtrl->TicksBeforeStart --;

	//	return;

	//}

	//выдерживаю просто какую-то паузу (например: при достижении начала строки)
	if(pCtrl->_tickCount != 0)
	{
		pCtrl->_tickCount --;

		return;

	}

	////нормализация шага: либо 1 либо -1, но не -5 и не 6 и тп.
	//if(pCtrl->_Step > 1)
	//{
	//	pCtrl->_Step = 1;

	//}
	//else if(pCtrl->_Step < -1)
	//{
	//	pCtrl->_Step = -1;

	//}

	//двигаю только, если строка длиннее окошка
	if(pCtrl->Base.Bounds.Width < strlen(pCtrl->pString))
	{
		if(pCtrl->Base.Bounds.Width == strlen(pCtrl->_pCurrentPosition) + endingSpacesNumber)
		{
			//pCtrl->_Step = -1;
			pCtrl->_pCurrentPosition = pCtrl->pString;

			pCtrl->_tickCount = pCtrl->TicksAtStringBeginning;

		}
		else// if(pCtrl->pString == pCtrl->_pCurrentPosition)
		{
//			pCtrl->_Step = 1;

			pCtrl->_pCurrentPosition += 1;

		}

//		pCtrl->_pCurrentPosition += pCtrl->_Step;

		////при достижении начала строки нужно выдержать паузу
		//if(pCtrl->pString == pCtrl->_pCurrentPosition)
		//{
		//	pCtrl->_tickCount = pCtrl->TicksAtStringBeginning;

		//}

	}

	memset(buffer, ' ', sizeof(buffer));

	//эта конструкция чтобы вовремя приделать сзади к прокручиваемой строке несколько пробелов
	if(strlen(pCtrl->_pCurrentPosition) >= pCtrl->Base.Bounds.Width)
	{
		strncpy(buffer, pCtrl->_pCurrentPosition, sizeof(buffer)-1);

	}
	else
	{
		memcpy(buffer, pCtrl->_pCurrentPosition, strlen(pCtrl->_pCurrentPosition));

	}

	buffer[pCtrl->Base.Bounds.Width] = 0;

	wherexy(&x, &y);

	gotoxy(pCtrl->Base.Bounds.X, pCtrl->Base.Bounds.Y);

	puts(buffer);

	gotoxy(x, y);

}


//------------------------------------------------------------------------------------



struct Type_MemViewCallbackParam
{
	char *startAddress;
	struct TextBox *pTextBox;

};

struct CtrlBase* _MemViewCallback(struct VirtualList *pList, unsigned int nRow)
{
	struct TextBox* box = ((struct Type_MemViewCallbackParam*)pList->Base.SomePointer)->pTextBox;
	char* baseAddr = ((struct Type_MemViewCallbackParam*)pList->Base.SomePointer)->startAddress;

	int i = 0;
	int bytesPerRow = 4;

	char hexView[15];
	char textView[15];
	memset(hexView, 0, sizeof(hexView));
	memset(textView, 0, sizeof(textView));

	//TODO: добавить рядом текстовое представление с заменой пробелом непечатаемых символов
	//0x20 .. 0x79, исключая 0x7B

	for(i = 0; i < bytesPerRow; i++)
	{
		int d = *(baseAddr + nRow*bytesPerRow + i) & 0xFF;
		//int d = ch & 0xFF;
		sprintf(hexView + 3*i, "%.2X ", d);

		if(0x20 <= d /*&& d <= 0x79*/ && d != 0x7B)
		{
			sprintf(textView+strlen(textView), "%c", d);

		}
		else
		{
			sprintf(textView+strlen(textView), " ");

		}

	}
	
	hexView[3*bytesPerRow] = 0;
	textView[bytesPerRow] = 0;

	strcpy(box->pValue, hexView);
	strcat(box->pValue, textView);
//	DEFINE_strcat_justify(box->pValue, textView, pList->Base.Bounds.Width);
//	*(box->pValue + 3*bytesPerRow + bytesPerRow) = 0;

	return (struct CtrlBase*)box;

}

char MemView(void* pMem)
{
	struct VirtualList memList;
	struct TextBox textBox;
	char buf[25];
	struct Type_MemViewCallbackParam callbackParam;
	unsigned char screenShot[128*64/8];
	VList_Constructor(&memList);
	TextBox_Constructor(&textBox, buf);

	memList.ItemsCount = 100;
	memList.AllowCyclic = 0;

	callbackParam.pTextBox = &textBox;
	callbackParam.startAddress = (char*)pMem;
	memList.Base.SomePointer = &callbackParam;
	memList.OnGetMemberData = _MemViewCallback;

	get_image(0, 0, 128, 64, screenShot);

	{
		int prevVideoMode = GetVideoMode();
		int prevFont = GetFont();

		SetVideoMode(VIDEO_NORMAL);
		SetFont(FONT_6X8);

		clr_scr();

		redraw_ctrl((struct CtrlBase*)&memList);

		while(1)
		{
			char key = GetInput(NULL);

			if(key == KEY_UP)
			{
				VList_MovePrevious(&memList);

			}
			else if(key == KEY_DOWN)
			{
				VList_MoveNext(&memList);

			}
			else if(key == KEY_ENTER || key == KEY_ESC)
			{
				clr_scr();

				show_image(0, 0, 128, 64, screenShot);

				SetVideoMode(prevVideoMode);
				SetFont(prevFont);

				if(key == KEY_ENTER)
				{
					return 0;

				}
				else
				{
					return -1;

				}

			}

		}

	}

}


//------------------------------------------------------------------------------------

//стандартный вывод прогресса (если не хочется создавать под свою задачу новый, то можно использовать этот)
void StandardShowProgress(unsigned int percentage)
{
	SetVideoMode(VIDEO_NORMAL);
	SetFont(FONT_6X8);

	clr_scr();

	printf("%d%%", percentage);

}












