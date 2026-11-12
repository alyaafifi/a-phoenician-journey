using System;
using System.Collections.Generic;
using PhoenicianJourney.Landing;
using PhoenicianJourney.Player;
using PhoenicianJourney.Trade;
using PhoenicianJourney.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace PhoenicianJourney
{
    /// <summary>
    /// Runs the harbor scene: the debug HUD, station prompts, the E key, and the cargo screen.
    /// The scene builder (Tools ▸ Phoenician Journey ▸ Rebuild Harbor Scene) wires the references.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HarborGame : MonoBehaviour
    {
        [SerializeField] private FirstPersonWalker player;
        [SerializeField] private StyleSheet styles;

        private readonly Dictionary<StationId, StationAction> stationActions = new Dictionary<StationId, StationAction>();
        private UIDocument document;
        private CargoScreen cargoScreen;
        private DyeScreen dyeScreen;
        private Label prompt;
        private Label status;
        private Label debug;
        private Station nearby;
        private float hudTimer;

        public CargoScreen Cargo => cargoScreen;

        public DyeScreen Dye => dyeScreen;

        private bool InteractionOpen => cargoScreen.IsOpen || dyeScreen.IsOpen;

        public Station NearbyStation => nearby;

        public string PromptText => prompt?.text ?? string.Empty;

        public void Configure(FirstPersonWalker walker, StyleSheet styleSheet)
        {
            player = walker;
            styles = styleSheet;
        }

        private sealed class StationAction
        {
            public StationAction(Func<string> prompt, Action run)
            {
                Prompt = prompt;
                Run = run;
            }

            public Func<string> Prompt { get; }
            public Action Run { get; }
        }

        private void Awake()
        {
            document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = document.rootVisualElement;
            root.Clear();
            if (styles != null)
            {
                root.styleSheets.Add(styles);
            }
            root.AddToClassList("harbor-root");

            var hud = new VisualElement { pickingMode = PickingMode.Ignore };
            hud.AddToClassList("hud");
            hud.Add(new Label("A Phoenician Journey · harbor") { pickingMode = PickingMode.Ignore });
            status = new Label(string.Empty) { pickingMode = PickingMode.Ignore };
            status.AddToClassList("hud-status");
            debug = new Label(string.Empty) { pickingMode = PickingMode.Ignore };
            debug.AddToClassList("hud-debug");
            var help = new Label(
                "Click or drag to look around · WASD or arrows to walk · Shift to run · E use station · R respawn · Esc frees the mouse")
            {
                pickingMode = PickingMode.Ignore,
            };
            help.AddToClassList("hud-help");
            hud.Add(status);
            hud.Add(debug);
            hud.Add(help);
            root.Add(hud);

            prompt = new Label(string.Empty) { pickingMode = PickingMode.Ignore };
            prompt.AddToClassList("prompt");
            prompt.style.display = DisplayStyle.None;
            root.Add(prompt);

            cargoScreen = new CargoScreen(root, TradeState.Shared);

            dyeScreen = new DyeScreen(root);
            stationActions[StationId.Cargo] = new StationAction(
                () => TradeState.Shared.ShipmentSent ? "Press E to see the shipment" : "Press E to load the ship",
                OpenCargo);
            stationActions[StationId.Dye] = new StationAction(
                () => TradeState.Shared.DyeTaskComplete ? "Press E to practice dyeing" : "Press E to dye a swatch",
                OpenDye);
        }

        public void SetStatus(string text) => status.text = text;

        /// <summary>Uses the station in reach, as the E key does.</summary>
        public void UseNearbyStation()
        {
            if (!InteractionOpen && nearby != null && stationActions.TryGetValue(nearby.Id, out var action))
            {
                action.Run();
                UpdatePrompt();
            }
        }

        private void OpenCargo()
        {
            player.SetMovementEnabled(false);
            cargoScreen.Open(() => player.SetMovementEnabled(true));
        }

        private void OpenDye()
        {
            player.SetMovementEnabled(false);
            dyeScreen.Open(finished =>
            {
                player.SetMovementEnabled(true);
                if (finished && TradeState.Shared.CompleteDyeTask())
                {
                    SetStatus("The dyer’s prepared batch of purple cloth is ready to ship.");
                }
            });
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && !InteractionOpen)
            {
                if (keyboard.eKey.wasPressedThisFrame)
                {
                    UseNearbyStation();
                }
                else if (keyboard.rKey.wasPressedThisFrame)
                {
                    player.Respawn();
                }
            }

            UpdatePrompt();

            hudTimer += Time.unscaledDeltaTime;
            if (hudTimer >= 0.1f)
            {
                hudTimer = 0f;
                var feet = player.Feet;
                debug.text =
                    $"Feet  {feet.x:0.00}, {feet.y:0.00}, {feet.z:0.00} m\n" +
                    $"{(player.Grounded ? "Grounded" : "Airborne")} · {player.HorizontalSpeed:0.0} m/s · {1f / Mathf.Max(Time.smoothDeltaTime, 0.0001f):0} fps";
            }
        }

        private void UpdatePrompt()
        {
            nearby = null;
            if (!InteractionOpen)
            {
                var best = Station.Reach;
                foreach (var station in Station.Active)
                {
                    if (!stationActions.ContainsKey(station.Id))
                    {
                        continue;
                    }

                    var distance = station.DistanceTo(player.Feet);
                    if (distance <= best)
                    {
                        best = distance;
                        nearby = station;
                    }
                }
            }

            var text = nearby != null ? stationActions[nearby.Id].Prompt() : string.Empty;
            if (prompt.text != text)
            {
                prompt.text = text;
                prompt.style.display = text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
