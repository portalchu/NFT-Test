using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.Extensions;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using UnityEngine;
using UnityEngine.UI;

public class NFTTest : MonoBehaviour
{
    public string URL = "http://127.0.0.1:7545";
    public string PrivateKey = "4fa243a5b6accae59b364fb22cab02a17d91ad082f65cfde9a19614fd75446e5";
    public string Account = "0xFA4Fac39E34aEb1b66Ffc731D443C510945e47e4";
    public string AddressTo = "0x840b83b6aE5321E5f61f2c8376970f2D8eF987D7";
    public decimal Amount = 1.1m;
    public decimal GasPriceGwei = 2;
    public string TransactionHash = "";
    public decimal BalanceAddressTo = 0m;
    public string ContractAddress = "0xcf0a57Bf77562f6Da51b97B96Bd534d5a6dd4dcf";
    public string nftURL = "ipfs://bafkreiem3gdjd57xgqhpi35ckpmumn4d4phbhi6zoiyjqldyiqjmejvf64";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetBlockNumberRequest()
    {
        StartCoroutine(GetBlockNumber());
    }

    public void TransferRequest()
    {
        StartCoroutine(EtherTranse());
    }

    public void TokenRequest()
    {
        StartCoroutine(DeployAndTransferToken());
    }

    public IEnumerator EtherTranse()
    {

        EthTransferUnityRequest etherTrans = new EthTransferUnityRequest(URL, PrivateKey, 444444444500);

        Debug.Log("EthTransferUnityRequest is go");
        Debug.Log("etherTrans : " + etherTrans);

        etherTrans.UseLegacyAsDefault = true;

        Debug.Log("Amount : " + Amount);
        Debug.Log("GasPriceGwei : " + GasPriceGwei);

        yield return etherTrans.TransferEther(AddressTo, Amount, GasPriceGwei);
        if (etherTrans.Exception != null)
        {
            Debug.Log("etherTrans faild!");
            Debug.Log(etherTrans.Exception.Message);
            yield break;
        }

        string transactionHash = etherTrans.Result;
        //create a poll to get the receipt when mined

        Debug.Log("transactionHash : " + transactionHash);

        TransactionReceiptPollingRequest transactionReceiptPolling = 
            new TransactionReceiptPollingRequest(URL);
        //checking every 2 seconds for the receipt
        yield return transactionReceiptPolling.PollForReceipt(transactionHash, 2);

        EthGetBalanceUnityRequest balanceRequest = new EthGetBalanceUnityRequest(URL);
        yield return balanceRequest.SendRequest(AddressTo, BlockParameter.CreateLatest());

        Debug.Log("Balance of account:" + 
            UnitConversion.Convert.FromWei(balanceRequest.Result.Value));
    }

    public IEnumerator GetBlockNumber()
    {
        EthBlockNumberUnityRequest blockNumberRequest = new EthBlockNumberUnityRequest(URL);

        yield return blockNumberRequest.SendRequest();

        Debug.Log(blockNumberRequest.Result.Value.ToString());
    }

    [Function("awardItem", "uint256")]
    public class BalanceOfFunction : FunctionMessage
    {
        
        [Parameter("address", "player", 1)]
        public string Owner { get; set; }

        [Parameter("string", "tokenURI", 2)]
        public string TokenUrl { get; set; }
    }

    [FunctionOutput]
    public class BalanceOfFunctionOutput : IFunctionOutputDTO
    {
        [Parameter("uint256", 1)]
        public BigInteger Token { get; set; }
    }

    [Function("balanceOf", "uint256")]
    public class Balance : FunctionMessage
    {

        [Parameter("address", "owner", 1)]
        public string Owner2 { get; set; }
    }

    [FunctionOutput]
    public class BalanceOutput : IFunctionOutputDTO
    {
        [Parameter("uint256", 1)]
        public BigInteger Token2 { get; set; }
    }

    //Sample of new features / requests
    public IEnumerator DeployAndTransferToken()
    {
        Debug.Log("Toekn Create!");
        Debug.Log("Account : " + Account);
        Debug.Log("nftURL : " + nftURL);
        Debug.Log("ContractAddress : " + ContractAddress);

        //Query request using our acccount and the contracts address (no parameters needed and default values)
        var queryRequest = new QueryUnityRequest<BalanceOfFunction, BalanceOfFunctionOutput>(URL, Account);
        var balanceOfFunction = new BalanceOfFunction();
        balanceOfFunction.Owner = Account;
        Debug.Log("Owner : " + balanceOfFunction.Owner);
        balanceOfFunction.TokenUrl = nftURL;
        Debug.Log("Owner : " + balanceOfFunction.TokenUrl);

        yield return queryRequest.Query(balanceOfFunction, ContractAddress);

        //Getting the dto response already decoded
        var dtoResult = queryRequest.Result;
        Debug.Log("Token : " + dtoResult.Token);

        var queryRequest2 = new QueryUnityRequest<Balance, BalanceOutput>(URL, Account);
        yield return queryRequest2.Query(new Balance() { Owner2 = Account}, ContractAddress);

        //Getting the dto response already decoded
        var dtoResult2 = queryRequest2.Result;
        Debug.Log("Token num : " + dtoResult2.Token2);
    }
}
