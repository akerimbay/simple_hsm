#include <AES.h>
#define VER "SIMPLE HSM V1.0\n"

#include "key.h"
#include "iv.h"

AES aes;
unsigned char Key[16];
unsigned char iv[16];
unsigned char tmp_iv[16];
unsigned char InputByte;
unsigned char BufferLength;
unsigned int BufferIndex;

unsigned char InputBuffer[128];
unsigned char OutputBuffer[128];

unsigned char KeyNumber;
unsigned char VectorNumber;

unsigned int i,j;
char ca[80];
String a;
unsigned char CommandBuffer[128];

void setup() {
	Serial.begin(115200);//115200 74880 57600 19200 9600
	Serial.print(VER);
	
	CleaBuffers();
	StartInput:
		ClearInputBuffer();
	
	Loop:
		if (Serial.available()) {
			InputByte = Serial.read();
			if(InputByte == 0x0a) goto Loop;
			if(InputByte != 0x0d) {
				if(InputByte != 0x20) { 
					CommandBuffer[BufferIndex] = toUpperCase(InputByte);
					BufferIndex++;
					if(BufferIndex == 1024) 
						BufferIndex = 0;
				}
			}
			else {
				if(BufferIndex % 2) {
					j=1;
					for(int i = 1; i < BufferIndex; i = i + 2){
						CommandBuffer[j] = hex2byte(CommandBuffer[i],CommandBuffer[i+1]);
						j++;
					}
					goto RunCommand;
				}
				else {
					Serial.print(BufferIndex,DEC); Serial.print(F(" Not enough characters in the command\n"));
					goto StartInput;
				}
			}
		}
		goto Loop;

	RunCommand:	
		BufferLength = j;
		if(CommandBuffer[0] != 0x24){
			Serial.print(F("No start symbol '$'\n"));
			goto StartInput;
		}
		else
			switch (CommandBuffer[1]) {
				case 0x00: {
					CleaBuffers();
					Serial.print(VER);
				}break;
				
				case 0x10: {
					for(i = 0; i < 16; i++) InputBuffer[i] = 0x00;
					for(i = 0; i < 16; i++) tmp_iv[i] = iv[i];
					
					for (i = 0; i < 16; i++) {
						InputBuffer[i] = CommandBuffer[i+2];
                    }
					
					aes.clean();
					aes.set_key(Key,128);
					aes.cbc_encrypt(InputBuffer, OutputBuffer, 1, tmp_iv);
					
					OutString(OutputBuffer);
				}break;
				
				case 0x20: {
					for(i = 0; i < 16; i++) InputBuffer[i] = 0x00;
					for(i = 0; i < 16; i++) tmp_iv[i] = iv[i];
					
					for (i = 0; i < 16; i++) {
						InputBuffer[i] = CommandBuffer[i+2];
                    }
					
					aes.clean();
					aes.set_key(Key,128);
					aes.cbc_decrypt(InputBuffer, OutputBuffer, 1, tmp_iv);
					
					OutString(OutputBuffer);
				}break;
				
				case 0x30: {
					KeyNumber = CommandBuffer[2];
					VectorNumber = CommandBuffer[3];

					for(i = 0; i < 16; i++) Key[i] = pgm_read_word(&KeyArray[KeyNumber][i]);
					for(i = 0; i < 16; i++) iv[i] = pgm_read_word(&VectorArray[VectorNumber][i]);
					
					Serial.print("00\n");
				}break;
			}
	goto StartInput;
}	

void loop() {
  // put your main code here, to run repeatedly:

}

void CleaBuffers(void){
	KeyNumber = 0;
    VectorNumber = 0;
	for(i = 0; i < 16; i++) Key[i] = pgm_read_word(&KeyArray[KeyNumber][i]);
	for(i = 0; i < 16; i++) iv[i] = pgm_read_word(&VectorArray[VectorNumber][i]);
	for(i = 0; i < 128; i++) CommandBuffer[i] = 0x00;
	InputByte = 0x00;
	BufferIndex = 0x00;
	BufferLength = 0x00;
	a = "";
}

void ClearInputBuffer(void) {
	for(i = 0; i < 128; i++) CommandBuffer[i] = 0x00;
	InputByte = 0x00;
	BufferIndex = 0x00; 
}

unsigned char hex2nibl(unsigned char hexchar) {
	unsigned char nibl;
    if((hexchar > 0x2F) && (hexchar < 0x3A)) {
       nibl = hexchar - 0x30;
    }
    if((hexchar > 0x40) && (hexchar < 0x47)) {
        nibl = hexchar - 0x37;
    }
    return nibl & 0x0F;
}

unsigned char hex2byte(unsigned char char1, unsigned char char0) {
    return (hex2nibl(char1) <<4) + hex2nibl(char0);
}

void OutString(unsigned char * Buffer) {
	a = "";
	for(i = 0; i < 16; i++) {
		sprintf(ca,"%02X",Buffer[i]);
		a = a + ca;
	}
	a = a + '\n';
	Serial.print(a);
}
