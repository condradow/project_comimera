package sc.player;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.api.plugins.IGameState;
import sc.plugin2026.Move;
import sc.plugin2026.GameState;
import sc.shared.GameResult;

import java.util.List;


/**
 * Das Herz des Clients:
 * Eine simple Logik, die zufaellige gueltige Zuege macht.
 * <p>
 * Ausserdem werden zum Spielverlauf Konsolenausgaben gemacht.
 */
public class Logic implements IGameHandler {
  private static final Logger log = LoggerFactory.getLogger(Logic.class);

  /** Aktueller Spielstatus. */
  private GameState gameState;

  /** In dieser Methode habt ihr 2 Sekunden (berechnet etwas Puffer ein) Zeit,
   * um euren nächsten Zug zu planen. */
  @Override
  public Move calculateMove() {
    long startTime = System.currentTimeMillis();
    log.info("Es wurde ein Zug von {} angefordert.", gameState.getCurrentTeam());

    // ### Das hier kann von euch angepasst werden ### //
    Logic_CWC logic_CWC = new Logic_CWC(gameState);
    Logic_MG logic_MG = new Logic_MG(gameState);
    Logic_RDP logic_RDP = new Logic_RDP(gameState);

    // ### hier könnt ihr euren move Hinterlegen, dieser muss returned werden ### //
    Move ourMove = logic_CWC.calculateMove();


    List<Move> possibleMoves = gameState.getSensibleMoves();
    // Hier intelligente Strategie zur Auswahl des Zuges einfügen
    Move move = possibleMoves.get((int) (Math.random() * possibleMoves.size())); // <

    log.info("Sende {} nach {}ms.", move, System.currentTimeMillis() - startTime);
    return move;
  }

  /** Ein neuer Spielstatus ist verfügbar, d.h. ein Zug wurde erfolgreich ausgeführt. */
  @Override
  public void onUpdate(IGameState gameState) {
    this.gameState = (GameState) gameState;
    log.info("Zug: {} Dran: {}", gameState.getTurn(), gameState.getCurrentTeam());
  }

  /** Wird aufgerufen, wenn das Spiel beendet ist. */
  public void onGameOver(GameResult data) {
    log.info("Das Spiel ist beendet, Ergebnis: {}", data);
  }

  /** Wird aufgerufen, wenn der Server einen Fehler meldet.
   * Bedeutet auch den Abbruch des Spiels. */
  @Override
  public void onError(String error) {
    log.warn("Fehler: {}", error);
  }
}
