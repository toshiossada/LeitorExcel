using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LeitorExcel
{
    public partial class frmGeradorScript : Form
    {
        DateTime _dataHoraAbertura;
        DateTime _dataHoraAberturaProtocolo;
        private const string Path = @"C:\Temp\protocolo.sql";
        private const string PathXls = @"C:\Temp\H.xlsx";
        private const string PathPeticao = @"C:\Temp\peticao.sql";
        private const string PathProtocolo = @"C:\Temp\protocolo.sql";
        public frmGeradorScript()
        {
            InitializeComponent();

            lblProgresso.Text = "";
            lblTimer.Text = "00:00:00";
            lblProgressoProtocolo.Text = "";
            lblTimerProtocolo.Text = "00:00:00";
        }
        private void MontarScripts()
        {
            _dataHoraAbertura = DateTime.Now;
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += timer_Tick;
            timer.Start();

            var conexao = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={PathXls};Extended Properties='Excel 12.0 Xml;HDR=YES';");
            if (File.Exists(PathPeticao)) File.Delete(PathPeticao);

            MontarPeticao(conexao);


            timer.Stop();
            MessageBox.Show($@"Geração de scripts de Petição Finalizado!", "FIM", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void MontarScriptsProtocolo()
        {
            _dataHoraAberturaProtocolo = DateTime.Now;
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += timerProtocolo_Tick;
            timer.Start();

            var conexao = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={PathXls};Extended Properties='Excel 12.0 Xml;HDR=YES';");
            if (File.Exists(PathProtocolo)) File.Delete(PathProtocolo);

            MontarProtocoloPeticao(conexao);

            timer.Stop();
            MessageBox.Show($@"Geração de scripts de protocolo Finalizado!", "FIM", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void MontarPeticao(OleDbConnection conexao)
        {
            var adapter = new OleDbDataAdapter("select * from [Petições$]", conexao);
            var ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                var i = 1;

                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    this.lblProgresso.BeginInvoke((MethodInvoker)delegate () { this.lblProgresso.Text = $"{i}/{ds.Tables[0].Rows.Count}"; });

                    var (TB_RESSARCIMENTO_PETICAO, TB_RESSARCIMENTO_PETICAO_COMPLEMENTO,
                        TB_RESSARCIMENTO_PETICAO_HISTORICO) = MontarQueryPeticao(linha);

                    SalvarArquivoSQL(PathPeticao, $"{TB_RESSARCIMENTO_PETICAO}\r\n{TB_RESSARCIMENTO_PETICAO_COMPLEMENTO}\r\n{TB_RESSARCIMENTO_PETICAO_HISTORICO}\r\n");
                    i++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();
            }
        }
        private static (string, string, string) MontarQueryPeticao(DataRow linha)
        {
            var prazoPeticao = linha["Prazo Petição"].ToString().Split('/');
            var prazoEntrega = linha["Prazo Entrega Doc# Agente"].ToString().Split('/');

            var tbRessarcimentoPeticao = $@"INSERT INTO dbo.TB_RESSARCIMENTO_PETICAO(
	                            DT_PRAZO_PETICAO,
	                            CD_SEQ_OBRIGACAO,
	                            VL_VALOR_PETICAO,
	                            CD_SEQ_PETICAO_VINCULADA,
	                            CD_SEQ_STATUS_ENTREGA_DOC_AGENTE,
	                            CD_SEQ_STATUS_PETICAO,
	                            CD_SEQ_TIPO_PETICAO,
	                            CD_SEQ_ORIGEM_PETICAO,
	                            CD_SEQ_COMPANHIA,
	                            CD_SEQ_ESTADO,
	                            CD_ANALISTA_RESPONSAVEL,
	                            COD_EXTERNO, 
							    CD_SEQ_ANDAMENTO_PETICAO
                            ) VALUES(
	                            '{prazoPeticao[1]}/{prazoPeticao[0]}/{prazoPeticao[2]}',
	                            {linha["ID Obrigação Vinculada"]},
	                            {linha["Valor Petição"].ToString().Replace(".", "").Replace(",", ".")},
	                            {(string.IsNullOrEmpty(linha["Petição Vinculada"]?.ToString()) ? "NULL" : linha["Petição Vinculada"])},
	                            (SELECT TOP 1 CD_SEQ_DOMINIO FROM dbo.TB_dominio where DS_DOMINIO = '{linha["Status Entrega Doc# Agente"]}'),
                                (select TOP 1 A.CD_SEQ_DOMINIO from dbo.TB_dominio A INNER JOIN TB_TIPO_DOMINIO B ON A.CD_SEQ_TIPO_DOMINIO = B.CD_SEQ_TIPO_DOMINIO AND B.DS_TIPO_DOMINIO = 'Status Peticao' where A.DS_DOMINIO = '{linha["Status Petição"]}'),
	                            (SELECT TOP 1 CD_SEQ_DOMINIO  FROM dbo.TB_dominio where DS_DOMINIO = '{linha["Tipo Petição"]}'),
	                            (SELECT TOP 1 CD_SEQ_DOMINIO  FROM dbo.TB_dominio where DS_DOMINIO = '{linha["Origem Petição"]}'),
	                            (select TOP 1 CD_SEQ_COMPANHIA from dbo.TB_COMPANHIA where CD_SAP = '{linha["Empresa SAP"]}'),
	                            (SELECT top 1 CD_SEQ_UF from dbo.TB_UF where SG_UF = '{linha["UF Origem"]}'),
	                            (SELECT top 1 CD_USUARIO FROM dbo.TB_USUARIO where nm_usuario = '{linha["Analista R&R Responsável"]}'), --Verificar Se nao pode ser por CS
	                            {linha["ID Petição"]},
                                (SELECT top 1 CD_SEQ_DOMINIO FROM dbo.TB_dominio D INNER JOIN TB_TIPO_DOMINIO E ON D.CD_SEQ_TIPO_DOMINIO = E.CD_SEQ_TIPO_DOMINIO AND E.DS_TIPO_DOMINIO = 'Andamento Peticao'  where DS_DOMINIO='{linha["Andamento Petição"]}')
                            );";

            ///*Andamento Petição*/
            var tbRessarcimentoPeticaoHistorico = $@"
                INSERT INTO dbo.TB_RESSARCIMENTO_PETICAO_HISTORICO (
	                CD_SEQ_RESSARCIMENTO_PETICAO,
	                DT_OCORRENCIA_HISTORICO,
	                DT_LEMBRETE_HISTORICO,
	                CD_USUARIO
                ) VALUES(
	                (select MAX(CD_SEQ_RESSARCIMENTO_PETICAO) from dbo.TB_RESSARCIMENTO_PETICAO where COD_EXTERNO = {linha["ID Petição"]}),
	                getdate(),
	                getdate(),
	                'TR018445'
                );";

            var tbRessarcimentoPeticaoComplemento = $@"INSERT INTO dbo.TB_RESSARCIMENTO_PETICAO_COMPLEMENTO
                    (	
	                    CD_SEQ_RESSARCIMENTO_PETICAO,
	                    CD_STATUS_PETICAO,
	                    NM_STATUS_PETICAO,
	                    CD_ORIGEM_PETICAO,
	                    NM_ORIGEM_PETICAO,
	                    SG_UF,
	                    NM_UF,
	                    CD_TIPO_PETICAO,
	                    NM_TIPO_PETICAO,
	                    NM_DETENTOR_PETICAO,--Esse é o detentor de credito
	                    NM_ANALISTA_RESPONSAVEL,  
	                    NM_AGENTE_RESPONSAVEL,

	                    NM_COMPAHIA,
                        NM_ANDAMENTO_PETICAO
                    ) VALUES(
	                    (select MAX(CD_SEQ_RESSARCIMENTO_PETICAO) from TB_RESSARCIMENTO_PETICAO where COD_EXTERNO = {linha["ID Petição"]}),
	                    (select TOP 1 A.CD_DOMINIO from dbo.TB_dominio A INNER JOIN TB_TIPO_DOMINIO B ON A.CD_SEQ_TIPO_DOMINIO = B.CD_SEQ_TIPO_DOMINIO AND B.DS_TIPO_DOMINIO = 'Status Peticao' where A.DS_DOMINIO = '{linha["Status Petição"]}'),
	                    '{linha["Status Petição"]}',
	                    (SELECT TOP 1 CD_DOMINIO FROM dbo.TB_dominio where DS_DOMINIO='{linha["Origem Petição"]}'), 
	                    '{linha["Origem Petição"]}',
	                    '{linha["UF Origem"]}',
	                    (select TOP 1 C.NM_UF from dbo.TB_UF C WHERE C.SG_UF = '{linha["UF Origem"]}'),
	                    (SELECT TOP 1 CD_DOMINIO FROM dbo.TB_dominio where DS_DOMINIO='{linha["Tipo Petição"]}'),
	                    '{linha["Tipo Petição"]}',
	                    '{linha["Detentor Credito"]}', 
	                    '{linha["Analista R&R Responsável"]}', 
	                    '{linha["Agente Responsável"]}', 
	                    (select TOP 1 NM_RAZAO_SOCIAL from dbo.TB_COMPANHIA where CD_SAP = '{linha["Empresa SAP"]}'),
                        (SELECT top 1 DS_DOMINIO FROM dbo.TB_dominio D INNER JOIN TB_TIPO_DOMINIO E ON D.CD_SEQ_TIPO_DOMINIO = E.CD_SEQ_TIPO_DOMINIO AND E.DS_TIPO_DOMINIO = 'Andamento Peticao'  where DS_DOMINIO='{linha["Andamento Petição"]}')
                    )";

            return (tbRessarcimentoPeticao, tbRessarcimentoPeticaoComplemento, tbRessarcimentoPeticaoHistorico);
        }
        private void MontarProtocoloPeticao(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Protocolos Peticao$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                var i = 1;
                foreach (DataRow linha in ds.Tables[0].Rows)
                {

                    try
                    {
                        this.lblProgressoProtocolo.BeginInvoke((MethodInvoker)delegate ()
                       {
                           this.lblProgressoProtocolo.Text = $"{i}/{ds.Tables[0].Rows.Count}";
                       });
                        SalvarArquivoSQL(PathProtocolo, MontarQueryProtocoloPeticao(linha));
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($@"Erro ao acessar os dados: { e.Message } registro numero {i}");
                        throw;
                    }
                    finally
                    {
                        i++;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();
            }
        }
        private static string MontarQueryProtocoloPeticao(DataRow linha)
        {
            var dataProtocolo = linha["Data Protocolo"].ToString().Split('/');
            var script = $@"INSERT INTO dbo.TB_RESSARCIMENTO_PETICAO_PROTOCOLO (
	                            NR_PETICAO_PROTOCOLO,
	                            CD_SEQ_RESSARCIMENTO_PETICAO,
	                            DT_REGISTRO_PETICAO_PROTOCOLO,
	                            DT_PETICAO_PROTOCOLO,
	                            NM_ARQUIVO_PETICAO_PROTOCOLO,
	                            DS_CAMINHO_PETICAO_PROTOCOLO
                            )
                            VALUES(
	                            '{linha["Numero Protocolo"]}',
	                            (select MAX(CD_SEQ_RESSARCIMENTO_PETICAO) from dbo.TB_RESSARCIMENTO_PETICAO where COD_EXTERNO = {linha["ID Petição"]}),
	                            getdate(),
	                            {((dataProtocolo.Length < 3) ? "NULL" : $"'{dataProtocolo[1]}/{dataProtocolo[0]}/{dataProtocolo[2]}'") },
	                            '{linha["Doc# Protocolo"]}',
                                {
                                (string.IsNullOrEmpty(linha["Buscar SGT Obrigações"].ToString()) ? (String.IsNullOrEmpty(linha["Doc# Protocolo"].ToString())) ? "''" : $@"'\\cpclsfsr03\SoftsPRD5\SGT\PRD\Ressarcimento\CARGAPETICOES\{linha["Doc# Protocolo"]}'" :
                                $@"(SELECT TOP 1 ODu.DS_CAMINHO FROM dbo.TB_OBRIGACAO_DETALHES OD 
                                    INNER JOIN dbo.TB_OBRIGACAO_DOCUMENTO ODu ON OD.CD_SEQ_OBRIGACAO_DETALHES = ODu.CD_SEQ_OBRIGACAO_DETALHES 
                                    INNER JOIN dbo.TB_RESSARCIMENTO_PETICAO RP ON RP.CD_SEQ_OBRIGACAO = OD.CD_SEQ_OBRIGACAO AND RP.COD_EXTERNO = {linha["ID Petição"]}
                                    WHERE 
	                                    NM_ARQUIVO = '{linha["Doc# Protocolo"]}')")
                                }
                            );";


            return script;
        }
        private static string MontarQueryPareceres(DataRow linha)
        {
            var dataParecer = linha["Data Parecer"].ToString().Split('/');
            var dataCiencia = linha["Data Ciencia"].ToString().Split('/');
            var dataLiberacao = linha["Previsao Liberacao"].ToString().Split('/');

            var script = $@"INSERT INTO TB_RESSARCIMENTO_PETICAO_PARECER(
	                        CD_SEQ_RESSARCIMENTO_PETICAO,
	                        DT_PETICAO_PARECER,
	                        DS_DECISAO_PETICAO_PARECER,
	                        NR_PETICAO_PARECER,
	                        CD_PARECERISTA_PETICAO_PARECER,
	                        DT_CIENCIA_PETICAO_PARECER,
	                        DS_RESUMO_PETICAO_PARECER,
	                        VL_DEFERIDO_PETICAO_PARECER,
	                        VL_RECURSO_PETICAO_PARECER,
	                        VL_BAIXA_PETICAO_PARECER,
	                        DT_PREVISAO_LIBERACAO_PETICAO_PARECER,
                            NR_PETICAO_PARECER_EXTERNO
                        ) VALUES(
	                         (select MAX(CD_SEQ_RESSARCIMENTO_PETICAO) from TB_RESSARCIMENTO_PETICAO where COD_EXTERNO = {linha["ID Petição"]}),
	                        '{dataParecer[1]}/{dataParecer[0]}/{dataParecer[2]}',
	                        '{linha["Decisão Parecer"]}',
	                        {linha["Numero Parecer"]},
	                        '{linha["Parecerista"]}',
	                        '{dataCiencia[1]}/{dataCiencia[0]}/{dataCiencia[2]}',
	                        '{linha["Resumo Parecer"]}',
	                        {(decimal.TryParse(linha["Valor Deferido"]?.ToString().Replace(".", "").Replace(",", "."), out var d) ? d.ToString(CultureInfo.InvariantCulture) : "NULL")},
	                        {(decimal.TryParse(linha["Valor Indef# (Recurso)"]?.ToString().Replace(".", "").Replace(",", "."), out var e) ? e.ToString(CultureInfo.InvariantCulture) : "NULL")},
	                        {(decimal.TryParse(linha["Valor Indef# (Recurso)"]?.ToString().Replace(".", "").Replace(",", "."), out var f) ? f.ToString(CultureInfo.InvariantCulture) : "NULL")},
	                        '{dataLiberacao[1]}/{dataLiberacao[0]}/{dataLiberacao[2]}',
                            '{linha["Numero Parecer Externo"]}'
                        );";


            return script;
        }
       
        #region metodos não utilizados
        private void MontarProtocoloNFe(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Protocolo NFe$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    String x;
                    x = linha["ID Petição"].ToString();
                    x = linha["Data Protocolo"].ToString();
                    x = linha["Numero Protocolo"].ToString();
                    x = linha["Doc# Protocolo"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();

            }
        }
        private void MontarGestaoPareceres(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Gestao Pareceres$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    SalvarArquivoSQL(Path, MontarQueryPareceres(linha));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();

            }
        }
        private void MontarGestaoRecebimentos(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Gestao Recebimento$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    String x;
                    x = linha["ID Petição"].ToString();
                    x = linha["Data Recebimento"].ToString();
                    x = linha["Tipo Recebimento"].ToString();
                    x = linha["Valor Recebido"].ToString();
                    x = linha["Emissa NF Recebimento"].ToString();
                    x = linha["Comprovantes Recebimento"].ToString();
                    x = linha["Valor Indef# (Recurso)"].ToString();
                    x = linha["Valor Indef# (Baixa)"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();

            }
        }
        private void MontarHistOcorrencias(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Hist Ocor$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    String x;
                    x = linha["ID Petição"].ToString();
                    x = linha["Data Ocorrencia"].ToString();
                    x = linha["Observacoes"].ToString();
                    x = linha["Usuário"].ToString();
                    x = linha["Data Lembrete"].ToString();
                    x = linha["Prazo Recebimento"].ToString();
                    x = linha["Status Andamento"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();

            }
        }
        private void MontarNegociacao(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Negociação$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);
                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    String x;
                    x = linha["ID Negociacao"].ToString();
                    x = linha["Tipo Negociação"].ToString();
                    x = linha["Nome Negociacao"].ToString();
                    x = linha["Doc# Parecer Externo"].ToString();
                    x = linha["Doc# Termo de Acordo"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();

            }
        }
        private void MontarNegociaçãoParcelas(OleDbConnection conexao)
        {
            OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [Negociação - Parcelas$]", conexao);
            DataSet ds = new DataSet();

            try
            {
                conexao.Open();

                adapter.Fill(ds);



                foreach (DataRow linha in ds.Tables[0].Rows)
                {
                    String x;
                    x = linha["ID Negociacao"].ToString();
                    x = linha["Parcela"].ToString();
                    x = linha["Data Vencimento"].ToString();
                    x = linha["Ids Petições"].ToString();
                    x = linha["Valor Parcela"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao acessar os dados: { ex.Message }");
            }
            finally
            {
                conexao.Close();

            }
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(@"C:\Temp\PET");
            BuscaArquivos(dir);
            MessageBox.Show("Arquivos copiados com sucesso. ", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void timer_Tick(object sender, EventArgs e)
        {

            var diferencaDataHora = (DateTime.Now).Subtract(_dataHoraAbertura);
            lblTimer.BeginInvoke(method: (MethodInvoker)delegate ()
           {
               this.lblTimer.Text =
                   $@"{diferencaDataHora.Hours.ToString().PadLeft(2, '0')}:{
                           diferencaDataHora.Minutes.ToString().PadLeft(2, '0')
                       }:{diferencaDataHora.Seconds.ToString().PadLeft(2, '0')}";
           });
        }
        private void timerProtocolo_Tick(object sender, EventArgs e)
        {
            var diferencaDataHora = (DateTime.Now).Subtract(_dataHoraAberturaProtocolo);
            lblTimerProtocolo.BeginInvoke(method: (MethodInvoker)delegate ()
            {
                this.lblTimerProtocolo.Text =
                    $@"{diferencaDataHora.Hours.ToString().PadLeft(2, '0')}:{
                            diferencaDataHora.Minutes.ToString().PadLeft(2, '0')
                        }:{diferencaDataHora.Seconds.ToString().PadLeft(2, '0')}";
            });
        }
        protected void BuscaArquivos(DirectoryInfo dir)
        {
            // lista arquivos do diretorio corrente
            foreach (FileInfo file in dir.GetFiles())
            {
                // aqui no caso estou guardando o nome completo do arquivo em em controle ListBox
                // voce faz como quiser
                var arquivo = file.FullName;
                if (file.Name.EndsWith(".pdf"))
                    CopiarArquivoBinario(arquivo, $"C:\\Temp\\Arquivos\\{file.Name}");
            }

            // busca arquivos do proximo sub-diretorio
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                BuscaArquivos(subDir);
            }
        }
        private static bool CopiarArquivoBinario(string nomeArquivoOrigem, string nomeArquivoDestino)
        {
            if (File.Exists(nomeArquivoOrigem) == false)
            {
                MessageBox.Show("Não foi possível encontrar o arquivo Origem", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            try
            {
                Stream s1 = File.Open(@nomeArquivoOrigem, FileMode.Open);
                Stream s2 = File.Open(@nomeArquivoDestino, FileMode.Create);

                BinaryReader f1 = new BinaryReader(s1);
                BinaryWriter f2 = new BinaryWriter(s2);

                while (true)
                {
                    byte[] buf = new byte[10240];
                    int sz = f1.Read(buf, 0, 10240);
                    if (sz <= 0)
                        break;
                    f2.Write(buf, 0, sz);
                    if (sz < 10240)
                        break; // fim de arquivo
                }
                f1.Close();
                f2.Close();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Erro ao copiar a o arquivo ...: { ex.Message }", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }
        protected static void SalvarArquivoSQL(string path, string texto)
        {
            using (var file = File.AppendText(path))
            {
                file.WriteLine(texto);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var thread = new Thread(MontarScripts);

            thread.Start();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var threadProtocolo = new Thread(MontarScriptsProtocolo);

            threadProtocolo.Start();
        }
    }
}
