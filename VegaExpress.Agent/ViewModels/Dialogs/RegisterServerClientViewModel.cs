using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Reactive;

using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;

namespace VegaExpress.Agent.ViewModels.Dialog
{
    public interface IRegisterServerClientViewModel
    {
        ReactiveCommand<Unit, Unit>? Save { get; }
        ReactiveCommand<Unit, Unit>? Cancel { get; }
    }
    public class RegisterServerClientViewModel : IRegisterServerClientViewModel
    {
        [Reactive]
        public string? HostName { get; set; }
        [Reactive]
        public string? UserName { get; set; }
        [Reactive]
        public string? OS { get; set; }
        [Reactive]
        public string? OSArquitecture { get; set; }
        [Reactive]
        public int Processors { get; set; }
        [Reactive]
        public string? ProcessArquitecture { get; set; }
        [Reactive]
        public string? RAM { get; set; }
        [Reactive]
        public bool IsServer { get; set; }
        [Reactive]
        public bool IsClient { get; set; }
        [Reactive]
        public bool IsBlocked { get; set; }        

        public ReactiveCommand<Unit, Unit>? Save { get; }

        public ReactiveCommand<Unit, Unit>? Cancel { get; }


        private readonly IServerRepository serverClientRepository;

        public RegisterServerClientViewModel(IServerRepository serverClientRepository)
        {
            this.serverClientRepository = serverClientRepository;

            this.WhenAnyValue(x => x.IsServer)
                .Subscribe(isServer =>
                {
                    this.IsClient = !isServer;
                });

            this.WhenAnyValue(x => x.IsClient)
                .Subscribe(isClient =>
                {
                    this.IsServer = !isClient;
                });

            var canExecute = this.WhenAnyValue(
                x => x.HostName,
                x => x.UserName,
                (hostName, userName) =>
                    !string.IsNullOrWhiteSpace(hostName) &&
                    !string.IsNullOrWhiteSpace(userName));

            Save = ReactiveCommand.Create(() =>
            {
                ServerModel serverClientModel = new ServerModel();
                serverClientModel.HostName = HostName;
                serverClientModel.UserName = UserName;
                serverClientModel.OS = OS;
                serverClientModel.OSArquitecture = OSArquitecture;
                serverClientModel.Processors = Processors;
                serverClientModel.ProcessArquitecture = ProcessArquitecture;
                serverClientModel.RAM = RAM;
                serverClientModel.IsServer = IsServer;
                serverClientModel.IsClient = IsClient;
                serverClientModel.IsBlocked = IsBlocked;

                this.serverClientRepository.CreateAsync(serverClientModel);
            }, canExecute);
        }
    }
}