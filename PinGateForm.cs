namespace FirawynixDock;

internal sealed class PinGateForm : Form
{
    private readonly Label message;
    private readonly Button pinButton;
    private readonly Button verifyButton;
    private bool checking;

    public PinGateForm()
    {
        Text = "Fixar Firawynix Dock";
        ClientSize = new Size(490, 250);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(7, 38, 49);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);
        Icon = Icon.ExtractAssociatedIcon(Path.Combine(AppContext.BaseDirectory, "FirawynixDock.exe"))
            ?? SystemIcons.Application;

        Controls.Add(new Label
        {
            Text = "FIXE O DOCK NA BARRA DE TAREFAS",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 232, 243),
            Location = new Point(24, 22), Size = new Size(445, 34)
        });
        message = new Label
        {
            Text = "O Dock abre acima do próprio ícone. Para usá-lo, fixe o aplicativo na barra de tarefas do Windows.",
            Location = new Point(25, 77), Size = new Size(440, 77)
        };
        Controls.Add(message);

        pinButton = MakeButton("Fixar na barra", 25, 174, 165);
        pinButton.Click += async (_, _) => await RequestPinAsync();
        Controls.Add(pinButton);

        verifyButton = MakeButton("Já fixei · verificar", 200, 174, 165);
        verifyButton.Click += async (_, _) => await CheckPinAsync(manualConfirmation: true);
        Controls.Add(verifyButton);

        var exitButton = MakeButton("Sair", 375, 174, 90);
        exitButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(exitButton);

        Shown += async (_, _) => await CheckPinAsync(manualConfirmation: false);
    }

    private Button MakeButton(string text, int x, int y, int width)
    {
        var button = new Button
        {
            Text = text, Location = new Point(x, y), Size = new Size(width, 43),
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            BackColor = Color.FromArgb(13, 94, 111),
            ForeColor = Color.White
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(70, 211, 226);
        return button;
    }

    private async Task CheckPinAsync(bool manualConfirmation)
    {
        if (checking) return;
        checking = true;
        verifyButton.Enabled = false;
        try
        {
            var pinned = await TaskbarPinning.IsPinnedAsync();
            if (IsDisposed) return;
            if (pinned == true) { DialogResult = DialogResult.OK; return; }
            if (pinned is null)
            {
                if (manualConfirmation) { DialogResult = DialogResult.OK; return; }
                message.Text = "O Windows não conseguiu informar se o Dock está fixado. Clique com o botão direito no atalho do Dock e escolha ‘Fixar na barra de tarefas’. Depois confirme abaixo.";
                verifyButton.Text = "Já fixei · continuar";
            }
            else
            {
                message.Text = "O Dock ainda não está fixado. Clique em ‘Fixar na barra’ e confirme a solicitação do Windows. Se necessário, fixe pelo menu Iniciar e depois clique em ‘Já fixei · verificar’.";
                verifyButton.Text = "Já fixei · verificar";
            }
        }
        finally
        {
            checking = false;
            if (!IsDisposed) verifyButton.Enabled = true;
        }
    }

    private async Task RequestPinAsync()
    {
        if (checking) return;
        checking = true;
        pinButton.Enabled = false;
        try
        {
            var pinned = await TaskbarPinning.RequestPinAsync();
            if (IsDisposed) return;
            if (pinned == true) { DialogResult = DialogResult.OK; return; }
            message.Text = pinned == false
                ? "A fixação não foi confirmada. Fixe o Dock para abrir o painel."
                : "O pedido automático não está disponível neste Windows. Fixe o Dock pelo menu Iniciar e confirme abaixo.";
            if (pinned is null) verifyButton.Text = "Já fixei · continuar";
        }
        finally
        {
            checking = false;
            if (!IsDisposed) pinButton.Enabled = true;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.None) DialogResult = DialogResult.Cancel;
        base.OnFormClosing(e);
    }
}
