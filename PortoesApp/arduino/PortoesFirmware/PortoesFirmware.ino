/*
 * Firmware para Controle de 16 Portões com Arduino Mega
 * Recebe comandos pela porta serial e aciona o relé correspondente por 500ms (pulso).
 * 
 * Pinos Utilizados: 22 a 37
 */

// Quantidade de relés
const int NUM_RELES = 16;

// Configuração dos pinos (iniciando no 22 até o 37 no Arduino Mega)
const int PINOS_RELE[NUM_RELES] = {
  22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37
};

// Comandos correspondentes a cada relé, na exata ordem do arquivo de configuração do painel
const String COMANDOS[NUM_RELES] = {
  "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "10"
};

// Módulo de Relé costuma ser ativado em nível BAIXO (LOW = Liga, HIGH = Desliga)
// Se o seu módulo relé ligar com nível lógigo ALTO (HIGH), basta inverter as duas linhas abaixo.
#define RELE_LIGADO LOW
#define RELE_DESLIGADO HIGH

// Tempo em milissegundos que o relé fica acionado (simulando o apertar do botão do controle)
const int TEMPO_PULSO = 500; 

void setup() {
  // Inicia a comunicação serial a 9600 baud, mesma velocidade padrão do C#
  Serial.begin(9600);
  
  // Configura os pinos como saída e garante que iniciem desligados
  for(int i = 0; i < NUM_RELES; i++) {
    pinMode(PINOS_RELE[i], OUTPUT);
    digitalWrite(PINOS_RELE[i], RELE_DESLIGADO);
  }

  Serial.println("Arduino Mega Pronto. Aguardando comandos...");
}

void loop() {
  // Verifica se há dados disponíveis na porta serial
  if (Serial.available() > 0) {
    // Lê a string até encontrar uma quebra de linha (\n), que o C# envia usando WriteLine()
    String comandoRecebido = Serial.readStringUntil('\n');
    
    // Remove espaços em branco ou quebras de linha fantasmas (\r) da ponta
    comandoRecebido.trim();

    // Avalia o comando
    if (comandoRecebido.length() > 0) {
      Serial.print("Comando Recebido do PC: ");
      Serial.println(comandoRecebido);
      
      // Procura qual o índice do relé correspondente na nossa lista de comandos
      int indexRele = -1;
      for(int i = 0; i < NUM_RELES; i++) {
        // Usa equalsIgnoreCase para garantir que "A" vai funcionar se mandar "a"
        if (comandoRecebido.equalsIgnoreCase(COMANDOS[i])) {
          indexRele = i;
          break; // Achou o comando, sai do laço for para ganhar tempo
        }
      }

      // Se achou o comando cadastrado no índice
      if (indexRele != -1) {
        Serial.print("Acionando Rele na porta Mega: ");
        Serial.println(PINOS_RELE[indexRele]);
        
        // Pulso Típico de Portão Automático
        digitalWrite(PINOS_RELE[indexRele], RELE_LIGADO);
        delay(TEMPO_PULSO);
        digitalWrite(PINOS_RELE[indexRele], RELE_DESLIGADO);
        
        Serial.println("Acionamento Concluido.");
      } else {
        Serial.println("Alerta: Comando Invalido ou Desconhecido.");
      }
    }
  }
}
