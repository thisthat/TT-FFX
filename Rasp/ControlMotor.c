#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <stdio.h>
#include <unistd.h>
#include <ctype.h>
#include <sys/times.h>
#include <signal.h>
#include <string.h>
#include <wiringPi.h>

#define SERVER_PORT 1313
#define LINESIZE 80

//Pr0totipe
static struct sigaction act;
void catchExit(int nSign);
void forward(int steps);
void backward(int steps);
void setStep(int w1, int w2, int w3, int w4);

void jump();
void up();
void down();

int fd;
int enable_pin = 1;//GPIO 18
int coil_A_1_pin = 7;//GPIO 4
int coil_A_2_pin = 0;//GPIO 17
int coil_B_1_pin = 4;//GPIO 23
int coil_B_2_pin = 5;//GPIO 24
int _step = 30;
int _stepControl = 1;

void getData(int in) {
	char inputline[LINESIZE];
	int len, i;
	while ((len = recv(in, inputline, LINESIZE, 0)) > 0) {
		inputline[len] = '\0';
		fprintf(stderr, "%s (%d)\n", inputline, len);
		if (strncmp(inputline, "Step:", 5) == 0){
			char s[2];
			s[0] = inputline[5];
			s[1] = '\0';
			_stepControl = atoi(s);
		}
		else if (strncmp(inputline, "SU",len) == 0){
			fprintf(stderr, "Sposto il motore SU \n");
			backward(_stepControl);
		}
		else if (strncmp(inputline, "GIU",len) == 0){
			fprintf(stderr, "Sposto il motore GIU\n");
			forward(_stepControl);
		}
		else if (strncmp(inputline, "JUMP",len) == 0){
			forward(_step);
			backward(_step);
			fprintf(stderr, "Salto\n");
		}
		else{
			fprintf(stderr, "%s (%d)\n", len, inputline);
		}
		
	}
}

void forward(int steps){
	int i;
	for (i = 0; i < steps; i++){
		setStep(HIGH, LOW, HIGH, LOW);
		setStep(LOW, HIGH, HIGH, LOW);
		setStep(LOW, HIGH, LOW, HIGH);
		setStep(HIGH, LOW, LOW, HIGH);
	}
}
void backward(int steps){
	int i;
	for (i = 0; i < steps; i++){
		setStep(HIGH, LOW, LOW, HIGH);
		setStep(LOW, HIGH, LOW, HIGH);
		setStep(LOW, HIGH, HIGH, LOW);
		setStep(HIGH, LOW, HIGH, LOW);
	}
}

void setStep(int w1, int w2, int w3, int w4){
	digitalWrite(coil_A_1_pin, w1);
	digitalWrite(coil_A_2_pin, w2);
	digitalWrite(coil_B_1_pin, w3);
	digitalWrite(coil_B_2_pin, w4);
	delay(2);
}

int main(unsigned argc, char **argv) {

	//Wiring
	wiringPiSetup();
	pinMode(enable_pin, OUTPUT);
	pinMode(coil_A_1_pin, OUTPUT);
	pinMode(coil_A_2_pin, OUTPUT);
	pinMode(coil_B_1_pin, OUTPUT);
	pinMode(coil_B_2_pin, OUTPUT);
	digitalWrite(enable_pin, HIGH);

	/*digitalWrite(0,HIGH); delay(1500);
	digitalWrite(0,LOW);*/

	//Gestione segnali
	act.sa_handler = catchExit;
	sigfillset(&(act.sa_mask));
	sigaction(SIGINT, &act, NULL);

	int sock, client_len;
	struct sockaddr_in server, client;
	/* impostazione del transport end point */
	if ((sock = socket(AF_INET, SOCK_STREAM, 0)) == -1) {
		perror("chiamata alla system call socket fallita");
		exit(1);
	}
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = htonl(INADDR_ANY);
	server.sin_port = htons(SERVER_PORT);
	/* leghiamo l'indirizzo al transport end point */
	if (bind(sock, (struct sockaddr *)&server, sizeof server) == -1) {
		perror("chiamata alla system call bind fallita");
		exit(2);
	}
	listen(sock, 1);
	/* gestione delle connessioni dei clienti */
	fprintf(stderr, "Listening on port 1313.\n");
	while (1) {
		client_len = sizeof(client);
		if ((fd = accept(sock, (struct sockaddr *)&client, &client_len)) < 0) {
			perror("accepting connection");
			exit(3);
		}
		fprintf(stderr, "Aperta connessione.\n");
		//send(fd, "Client Connesso\n", 27, 0);
		getData(fd);
		close(fd);
		fprintf(stderr, "Chiusa connessione.\n");
	}
}

void catchExit(int nSign){
	if (nSign == SIGINT){
		fprintf(stderr, "Closing Socket.\n");
		close(fd);
		exit(0);
	}
}