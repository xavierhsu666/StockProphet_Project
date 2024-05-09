
import subprocess
import os

# 获取当前文件的绝对路径
current_file_path = os.path.abspath(__file__).replace('PyAPI.py','')
combinePath = os.path.join(current_file_path,'requirements.txt')



def install_requirements():
    try:
        # 执行 pip 命令安装 requirements.txt 中列出的所有依赖项
        subprocess.check_call(["pip", "install", "-r",combinePath])
        print("Requirements installed successfully!")
    except subprocess.CalledProcessError as e:
        print("Error occurred while installing requirements:", e)



try:
    from datetime import datetime
    import requests
    import pandas as pd
    import numpy as np
    from io import StringIO
    from sqlalchemy import create_engine, Numeric,text
    import warnings
except:
    # 调用函数来安装 requirements.txt 中的依赖项
    install_requirements()
    os.system('cls')
# 忽略所有警告
warnings.filterwarnings("ignore")

def  mutiTimesApi_call(stockNo,dates_input):
    print("---Call mutiTimesApi_call")
        
    dates=[]
    month=int(str(dates_input)[4:6])

    for i in range(12-month):
        dates.append(int(dates_input)-10000+(1+i)*100)
    for i in range(month):
        dates.append(int(dates_input)-100*(month-i-1))
    url_template1 = "https://www.twse.com.tw/exchangeReport/STOCK_DAY?response=html&date={}&stockNo={}"
    url_template2 = "https://www.twse.com.tw/rwd/zh/afterTrading/BWIBBU?response=html&date={}&stockNo={}"
    url_template3 = 'https://mops.twse.com.tw/nas/t21/sii/t21sc03_{}_0.html'


    # 建立一個空的 DataFrame 來存儲所有數據
    all_data = pd.DataFrame()

    for date in dates:
        # 取得第一個 API 的數據
        url1 = url_template1.format(date, stockNo)
        
        data1 = pd.read_html(StringIO(requests.get(url1).text))[0]
        data1.columns = data1.columns.droplevel(0)

        data1.insert(1, '股票代號', stockNo)
        
        # 加入每月營收
        head=int(str(date)[0:4])-1911
        month_in=int(str(date)[4:6])
        datee = f'{head}_{month_in}'
        url3 = url_template3.format(datee)
        res = requests.get(url3)
        res.encoding = 'big5'
        html_content = res.text
        try:
            html_df = pd.read_html(StringIO(html_content))
            apple=0
        except ValueError:
            # Handle the ValueError when no tables are found
            apple=1
            
        
        
        # Step3. 篩選出個股月營收資訊
        # 3.1 剃除行數錯誤的表格,並將表格合併
        month_df = pd.concat([df for df in html_df if df.shape[1] == 11]) 

    
        
        # 3.2 設定表格的 header 
        month_df.columns = month_df.columns.get_level_values(1)

        # 3.3 剃除多餘欄位, 重新排序索引值
        month_df = month_df[['公司 代號', '公司名稱', '當月營收']]
        month_df = month_df[month_df['公司名稱'] != '合計']
        month_df = month_df.reset_index(drop=True) 
        month_end_df = month_df.loc[month_df['公司 代號'] == stockNo]
    
        i=0
        key=0
        while len(month_end_df)==0:
            # If data for the current month is not available, fetch data for the previous month
            previous_month = dates[10-i]  # Subtracting 100 from the date to get the previous month
            i+=1
            head=int(str(previous_month)[0:4])-1911
            month_in=int(str(previous_month)[4:6])
            datee = f'{head}_{month_in}'
            url3 = url_template3.format(datee)
            res = requests.get(url3)
            res.encoding = 'big5'
            html_content = res.text
            html_df = pd.read_html(StringIO(html_content))
            month_df = pd.concat([df for df in html_df if df.shape[1] == 11])
            month_df.columns = month_df.columns.get_level_values(1)
            month_df = month_df[['公司 代號', '公司名稱', '當月營收']]
            month_df = month_df[month_df['公司名稱'] != '合計']
            month_df = month_df.reset_index(drop=True) 
            month_end_df = month_df.loc[month_df['公司 代號'] == stockNo]
            if i == 4:
                data1.insert(1, '公司名稱', 'ETF')
                data1.insert(1, '當月營收', np.NAN)
                key=1
                break
            # Fetch data for the previous month
        if key==0:
            data1.insert(1, '公司名稱', month_end_df.iloc[0,1])
            data1.insert(1, '當月營收', month_end_df.iloc[0,2])
        i=0
        

        # 取得第二個 API 的數據
        url2 = url_template2.format(date, stockNo)
        # html_content = requests.get(url2).text
        data2 = pd.read_html(StringIO(requests.get(url2).text))[0]
        
        data2.columns = data2.columns.droplevel(0)

        

        data2[['年', '季']] = data2['財報年/季'].str.split('/', expand=True)
        # 下午3點跑的時候 有機率會data1/2 對不上(更新時間不一樣)
        if(data1.shape[0]!=data2.shape[0]):
            # 要出錯囉~~晚點再試，等表1表2更新就好...才怪
            # 檢查看誰比較長，把比較長的刪除一行看看
            if(max(data1.shape[0],data2.shape[0])==data2.shape[0]):
                data2 = data2.drop(data2.index[-1])
            elif(max(data1.shape[0],data2.shape[0])==data1.shape[0]):
                data1 = data1.drop(data1.index[-1])
        data2.iloc[:,0]=data1.iloc[:,0]

        # Ensure both DataFrames have a common column for merging
        common_column = '日期'

        
        
    # =============================================================================
    #     if common_column not in data1.columns or common_column not in data2.columns:
    #         raise ValueError(f"Column '{common_column}' not found in both DataFrames.")
    # 
    # =============================================================================
        # 合併兩份數據
        merged_data = pd.merge(data1, data2, on=common_column, how='outer')
    # 將合併後的數據添加到 all_data 中
        all_data = pd.concat([all_data, merged_data])

    # SQL_data 是新的欄位設定名稱

    SQL_data = {"日期": "ST_Date",  
                "股票代號": "SN_Code",
                "成交股數": "STe_TradeQuantity",
                "成交金額": "STe_TradeMoney",
                "開盤價": "STe_Open",
                "最高價": "STe_Max",
                "最低價": "STe_Min",
                "收盤價": "STe_Close",
                "漲跌價差": "STe_SpreadRatio",
                "成交筆數": "STe_TransActions",
                "殖利率(%)": "SB_Yield",
                "股利年度": "STe_Dividend_Year",
                "本益比": "SI_PE",
                "股價淨值比":"SB_PBRatio",
                "財報年/季":"ST_Year_Quarter",
                "年":"ST_Year",
                "季":"ST_Quarter",
                "公司名稱":"SN_Name",
                "當月營收":"SB_BussinessIncome",
                }


    # 新框架裡面放入原始資料
    SQL_data_df = pd.DataFrame(all_data)

    # 使用 rename 函數進行欄位名稱轉換
    SQL_data_df.rename(columns=SQL_data, inplace=True)

    # 避免數值類的出現其他奇怪的字串，先全部轉為Nah
    SQL_data_df['STe_Close']=pd.to_numeric(SQL_data_df['STe_Close'], errors='coerce')
    SQL_data_df['STe_Open']=pd.to_numeric(SQL_data_df['STe_Open'], errors='coerce')
    SQL_data_df['STe_Max']=pd.to_numeric(SQL_data_df['STe_Max'], errors='coerce')
    SQL_data_df['STe_Min']=pd.to_numeric(SQL_data_df['STe_Min'], errors='coerce')
    SQL_data_df['STe_TradeQuantity']=pd.to_numeric(SQL_data_df['STe_TradeQuantity'])
    SQL_data_df['STe_TradeMoney']=pd.to_numeric(SQL_data_df['STe_TradeMoney'], errors='coerce')
    SQL_data_df['SI_PE'] = pd.to_numeric(SQL_data_df['SI_PE'], errors='coerce')
    SQL_data_df['STe_TransActions']=pd.to_numeric(SQL_data_df['STe_TransActions'])
    SQL_data_df['STe_Close'] = SQL_data_df['STe_Close'].fillna(SQL_data_df['STe_Close'].mean())
    SQL_data_df['STe_Open'] = SQL_data_df['STe_Open'].fillna(SQL_data_df['STe_Open'].mean())
    SQL_data_df['STe_Max'] = SQL_data_df['STe_Max'].fillna(SQL_data_df['STe_Max'].mean())
    SQL_data_df['STe_Min'] = SQL_data_df['STe_Min'].fillna(SQL_data_df['STe_Min'].mean())
    SQL_data_df['STe_TradeQuantity'] = SQL_data_df['STe_TradeQuantity'].fillna(SQL_data_df['STe_TradeQuantity'].mean())
    SQL_data_df['STe_TradeMoney'] = SQL_data_df['STe_TradeMoney'].fillna(SQL_data_df['STe_TradeMoney'].mean())
    SQL_data_df['SI_PE'] = SQL_data_df['SI_PE'].fillna(SQL_data_df['SI_PE'].mean())
    SQL_data_df['STe_TransActions'] = SQL_data_df['STe_TransActions'].fillna(SQL_data_df['STe_TransActions'].mean())

    # 指數五日平均線
    # 假設 SQL_data_df 是包含收盤價的 DataFrame
    # 使用當日的值填充空值
    # SQL_data_df['SI_MovingAverage_5'] = pd.to_numeric(SQL_data_df['STe_Close'].fillna(method='ffill').rolling(window=5).mean())
    SQL_data_df['SI_MovingAverage_5'] = pd.to_numeric(SQL_data_df['STe_Close'].ffill().rolling(window=5).mean(), errors='coerce')
    SQL_data_df['SI_MovingAverage_5'] = pd.to_numeric(SQL_data_df['SI_MovingAverage_5'], errors='coerce')

    # 指數三十日平均線
    # 使用當日的值填充前 29 天的空缺
    # SQL_data_df['SI_MovingAverage_30'] = SQL_data_df['STe_Close'].fillna(method='ffill').rolling(window=30).mean()
    SQL_data_df['SI_MovingAverage_30'] = SQL_data_df['STe_Close'].ffill().rolling(window=30).mean()
    SQL_data_df['SI_MovingAverage_30'] = pd.to_numeric(SQL_data_df['SI_MovingAverage_30'], errors='coerce')

    # RSV 五日線
    SQL_data_df['SI_RSV_5'] = ((SQL_data_df['STe_Close'] - SQL_data_df['STe_Close'].rolling(window=5).min())/(SQL_data_df['STe_Close'].rolling(window=5).max() - SQL_data_df['STe_Close'].rolling(window=5).min())) * 100
    SQL_data_df['SI_RSV_5'] = pd.to_numeric(SQL_data_df['SI_RSV_5'], errors='coerce')

    # RSV 三十日線
    SQL_data_df['SI_RSV_30'] = ((SQL_data_df['STe_Close'] - SQL_data_df['STe_Close'].rolling(window=30).min())/(SQL_data_df['STe_Close'].rolling(window=30).max() - SQL_data_df['STe_Close'].rolling(window=30).min())) * 100
    SQL_data_df['SI_RSV_30'] = pd.to_numeric(SQL_data_df['SI_RSV_30'], errors='coerce')

    # K 值五日線/三十日
    SQL_data_df['SI_K_5'] = SQL_data_df['SI_RSV_5'].copy()  # Create a copy of SI_RSV_5 as initial %K values
    SQL_data_df['SI_K_30'] = SQL_data_df['SI_RSV_30'].copy()  # Create a copy of SI_RSV_30 as initial %K values

    SQL_data_df=SQL_data_df.reset_index(drop=True)

    for i in range(1, len(SQL_data_df)):
        # Check if the previous day's %K value is NaN
        if pd.isna(SQL_data_df.at[i - 1, 'SI_K_5']):
            previous_k = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k = SQL_data_df.at[i - 1, 'SI_K_5']  # Use previous day's %K value
        if pd.isna(SQL_data_df.at[i - 1, 'SI_K_30']):
            previous_k_30 = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k_30 = SQL_data_df.at[i - 1, 'SI_K_30']  # Use previous day's %K value
        # Calculate the current day's %K using the modified formula
        current_k = (SQL_data_df.at[i, 'SI_RSV_5'] + previous_k * (5 - 1)) / 5
        current_k_30 = (SQL_data_df.at[i, 'SI_RSV_30'] + previous_k_30 * (5 - 1)) / 5
        # Update the %K value in the DataFrame
        SQL_data_df.at[i, 'SI_K_5'] = current_k
        SQL_data_df.at[i, 'SI_K_30'] = current_k_30

    # D 值五日線/三十日
    SQL_data_df['SI_D_5'] = SQL_data_df['SI_K_5'].copy()  # Create a copy of SI_RSV_5 as initial %K values
    SQL_data_df['SI_D_30'] = SQL_data_df['SI_K_30'].copy()  # Create a copy of SI_RSV_30 as initial %K values

    for i in range(1, len(SQL_data_df)):
        # Check if the previous day's %K value is NaN
        if pd.isna(SQL_data_df.at[i - 1, 'SI_D_5']):
            previous_k = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k = SQL_data_df.at[i - 1, 'SI_D_5']  # Use previous day's %K value
        if pd.isna(SQL_data_df.at[i - 1, 'SI_D_30']):
            previous_k_30 = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k_30 = SQL_data_df.at[i - 1, 'SI_D_30']  # Use previous day's %K value
        # Calculate the current day's %K using the modified formula
        current_k = (SQL_data_df.at[i, 'SI_K_5'] + previous_k * (5 - 1)) / 5
        current_k_30 = (SQL_data_df.at[i, 'SI_K_30'] + previous_k_30 * (5 - 1)) / 5
        # Update the %K value in the DataFrame
        SQL_data_df.at[i, 'SI_D_5'] = current_k
        SQL_data_df.at[i, 'SI_D_30'] = current_k_30


    # Define the period for EMA calculation
    ema_period = 12
    ema_period_long = 26

    # Calculate the first EMA value based on the average of 'STe_Close' for the first 7 days
    first_ema_value = SQL_data_df['STe_Close'].rolling(window=ema_period).mean()
    first_ema_value_long = SQL_data_df['STe_Close'].rolling(window=ema_period_long).mean()
    # Initialize the 'EMA' column with NaN values
    SQL_data_df['SI_ShortEMA'] = SQL_data_df['SI_LongEMA'] =  np.nan

    # Assign the calculated first EMA value to the first row of the 'EMA' column
    SQL_data_df.at[ema_period-1, 'SI_ShortEMA'] = first_ema_value.iloc[ema_period-1]
    SQL_data_df.at[ema_period_long-1, 'SI_LongEMA'] = first_ema_value_long.iloc[ema_period_long-1]
    # Calculate EMA for the following days using the specified formula
    for i in range(ema_period, len(SQL_data_df)):
        SQL_data_df.at[i, 'SI_ShortEMA'] = (2 * SQL_data_df.at[i, 'STe_Close'] / (ema_period + 1)) + ((ema_period - 1) * SQL_data_df.at[i - 1, 'SI_ShortEMA'] / (ema_period + 1))
        
    for i in range(ema_period_long, len(SQL_data_df)):
        SQL_data_df.at[i, 'SI_LongEMA']  = (2 * SQL_data_df.at[i, 'STe_Close'] / (ema_period_long+ 1)) + ((ema_period_long - 1) * SQL_data_df.at[i - 1, 'SI_LongEMA'] / (ema_period_long + 1))

    # Dif
    SQL_data_df['SI_Dif'] = SQL_data_df['SI_ShortEMA'] - SQL_data_df['SI_LongEMA']
    SQL_data_df['SI_Dif'] = pd.to_numeric(SQL_data_df['SI_Dif'], errors='coerce')

    # Define the period for MACD calculation
    macd_period = 9
    # Calculate the MACD

    SQL_data_df.at[ema_period_long+macd_period-2,'SI_MACD']=SQL_data_df['SI_Dif'][ema_period_long-1:ema_period_long+macd_period-1].sum()/9
        
    for i in range(ema_period_long+macd_period-1, len(SQL_data_df)):
        SQL_data_df.at[i, 'SI_MACD']=SQL_data_df.at[i - 1, 'SI_MACD']+2/(1+macd_period)*(SQL_data_df.at[i, 'SI_Dif']-SQL_data_df.at[i - 1, 'SI_MACD'])
        

    # OSC
    SQL_data_df['SI_OSC'] = ((SQL_data_df['STe_Close'] / SQL_data_df['STe_Close'].shift(9)) * 100) - 100
    SQL_data_df['SI_OSC'] = pd.to_numeric(SQL_data_df['SI_OSC'], errors='coerce')

    # MA
    SQL_data_df['SI_MA'] =  SQL_data_df['SI_OSC'].rolling(window=10).sum()/10
    SQL_data_df['SI_MA'] = pd.to_numeric(SQL_data_df['SI_MA'], errors='coerce')

    # EPS
    try:
        SQL_data_df['SB_EPS'] =  SQL_data_df['STe_Close'] / SQL_data_df['SI_PE']
        SQL_data_df['SB_EPS'] = pd.to_numeric(SQL_data_df['SB_EPS'], errors='coerce')
    except TypeError:
        # Handle the ValueError when no tables are found
        SQL_data_df['SB_EPS']=pd.to_numeric(0)

    # SQL Server 連線部分
    server_name = 'localhost'
    database_name = 'StockProphet'
    username = 'sa'
    password = 'sa'

    connection_string = f'mssql+pyodbc://{server_name}/{database_name}?trusted_connection=yes&driver=ODBC+Driver+17+for+SQL+Server'
    engine = create_engine(connection_string)



    # 將所有數據儲存到 SQL Server
    table_name = "Stock"
    SQL_data_df=SQL_data_df.reset_index(drop=True)

    for i in range(len(SQL_data_df)):
        # SQL_data_df.at[i,'ST_Date']=str(int(SQL_data_df.iloc[i][0].replace('/',''))+19110000)
        SQL_data_df.at[i,'ST_Date'] = str(int(SQL_data_df.iloc[i][0].replace('/',''))+19110000)
        if i==0:
            # SQL_data_df.at[i,'STe_SpreadRatio']=(SQL_data_df.iloc[i][7]-SQL_data_df.iloc[i][8])/(SQL_data_df.iloc[i][9]+float(SQL_data_df.iloc[i][8]))*100
            SQL_data_df.at[i,'STe_SpreadRatio'] = (SQL_data_df.iloc[i][7] - SQL_data_df.iloc[i][8]) / (SQL_data_df.iloc[i][9] + float(SQL_data_df.iloc[i][8])) * 100
        else:
            # SQL_data_df.at[i,'STe_SpreadRatio']=(SQL_data_df.iloc[i][7]-SQL_data_df.iloc[i][8])/SQL_data_df.iloc[i-1][9]*100
            SQL_data_df.at[i,'STe_SpreadRatio'] = (SQL_data_df.iloc[i][7] - SQL_data_df.iloc[i][8]) / SQL_data_df.iloc[i-1][9] * 100

    SQL_data_df['STe_SpreadRatio'] = pd.to_numeric(SQL_data_df['STe_SpreadRatio'], errors='coerce')

    # Replace NaN values with a default value (you can modify this based on your requirements)
    SQL_data_df['SI_MovingAverage_5'] = SQL_data_df['SI_MovingAverage_5'].fillna(SQL_data_df['STe_Close'])
    SQL_data_df['SI_MovingAverage_30'] = SQL_data_df['SI_MovingAverage_30'].fillna(SQL_data_df['STe_Close'])
    SQL_data_df['SI_RSV_5'] = SQL_data_df['SI_RSV_5'].fillna(SQL_data_df['SI_RSV_5'].mean())
    SQL_data_df['SI_RSV_30'] = SQL_data_df['SI_RSV_30'].fillna(SQL_data_df['SI_RSV_30'].mean())
    SQL_data_df['SI_K_5'] = SQL_data_df['SI_K_5'].fillna(SQL_data_df['SI_K_5'].mean())
    SQL_data_df['SI_K_30'] = SQL_data_df['SI_K_30'].fillna(SQL_data_df['SI_K_30'].mean())
    SQL_data_df['SI_D_5'] = SQL_data_df['SI_D_5'].fillna(SQL_data_df['SI_D_5'].mean())
    SQL_data_df['SI_D_30'] = SQL_data_df['SI_D_30'].fillna(SQL_data_df['SI_D_30'].mean())
    SQL_data_df['SI_ShortEMA'] = SQL_data_df['SI_ShortEMA'].fillna(SQL_data_df['SI_ShortEMA'].mean())
    SQL_data_df['SI_LongEMA'] = SQL_data_df['SI_LongEMA'].fillna(SQL_data_df['SI_LongEMA'].mean())
    SQL_data_df['SI_Dif'] = SQL_data_df['SI_Dif'].fillna(SQL_data_df['SI_Dif'].mean())
    SQL_data_df['SI_MACD'] = SQL_data_df['SI_MACD'].fillna(SQL_data_df['SI_MACD'].mean())
    SQL_data_df['SI_OSC'] = SQL_data_df['SI_OSC'].fillna(SQL_data_df['SI_OSC'].mean())
    SQL_data_df['SI_MA'] = SQL_data_df['SI_MA'].fillna(SQL_data_df['SI_MA'].mean())
    # SQL_data_df['S_PK'] = SQL_data_df['ST_Date'][0:4]+"-"+SQL_data_df['ST_Date'][4:6]+"-"+SQL_data_df['ST_Date'][6:8]+"_"+SQL_data_df['SN_Code']
    SQL_data_df['S_PK'] =  SQL_data_df['ST_Date'].str[0:4]+"-"+SQL_data_df['ST_Date'].str[4:6]+"-"+SQL_data_df['ST_Date'].str[6:8]+"_"+SQL_data_df['SN_Code']
    SQL_data_df['ST_Date']=  SQL_data_df['ST_Date'].str[0:4]+"-"+SQL_data_df['ST_Date'].str[4:6]+"-"+SQL_data_df['ST_Date'].str[6:8]
    SQL_data_df['ST_UpdateDate']= datetime.now().strftime("%Y-%m-%d")
    # 将'SI_PE'列中的值转换为数字，错误值将转换为0
    SQL_data_df['SI_PE'] = pd.to_numeric(SQL_data_df['SI_PE'], errors='coerce')

    # 计算除了NaN之外的平均值
    mean_without_nan = SQL_data_df['SI_PE'].dropna().mean()

    # 使用fillna将NaN填充为平均值
    SQL_data_df['SI_PE'] = SQL_data_df['SI_PE'].fillna(mean_without_nan)
    SQL_data_df['SI_PE'] = SQL_data_df['SI_PE'].fillna(0)



    # 從資料庫讀取目標表格的資料到 DataFrame 中
    existing_data_df = pd.read_sql('SELECT * FROM Stock where SN_Code = '+stockNo, engine)
    
    # 改順序
    SQL_data_df = SQL_data_df[existing_data_df.columns]
    
    # 找到目標表格中不存在的資料
    missing_data_df = pd.merge(SQL_data_df, existing_data_df[['S_PK']], on='S_PK', how='outer', indicator=True)
    missing_data_df = missing_data_df[missing_data_df['_merge'] == 'left_only'].drop(columns=['_merge'])
    missing_data_df.dropna(subset=['S_PK'], inplace=True)
    missing_data_df.drop_duplicates(subset=['S_PK'], inplace=True)

    # 如果有缺失的資料，將其插入到目標表格中
    if existing_data_df.empty:
        print("---資料庫沒值，全部灌進DB")
        missing_data_df.to_sql(table_name, engine, if_exists='append', index=False)
        print("---成功")
    else:
        if not missing_data_df.empty:
            print("---資料庫有值，並且有差異，將差異灌進DB")
            missing_data_df.to_sql(table_name, engine, if_exists='append', index=False)
            print("---成功")
            
        else:
            print("---資料庫有值，但無差異，不動作")

    # SQL_data_df.to_sql(table_name, engine, index=False, if_exists='replace') # Change 'replace' to 'append' if you want to append data

    
     
def oneTimeApi_call(stockNo,dates_input):
    print("---Call oneTimeApi_call")
    from dateutil.relativedelta import relativedelta
    dates=[]
    month=int(str(dates_input)[4:6])
    
    curTime = datetime.strptime(dates_input, '%Y%m%d')
    
    # 設定要跑幾個月
    lastN_Year = 3
    dates.append(int(dates_input))
    for i in range(1,lastN_Year+1,1):
        result_date = curTime - relativedelta(months=i)
        dates.append(int(result_date.strftime('%Y%m%d')))
    dates = dates[::-1]
    # webapi urls
    url_template1 = "https://www.twse.com.tw/exchangeReport/STOCK_DAY?response=html&date={}&stockNo={}"
    url_template2 = "https://www.twse.com.tw/rwd/zh/afterTrading/BWIBBU?response=html&date={}&stockNo={}"
    url_template3 = 'https://mops.twse.com.tw/nas/t21/sii/t21sc03_{}_0.html'

    # 建立一個空的 DataFrame 來存儲所有數據
    all_data = pd.DataFrame()

    for date in dates:
        # 取得第一個 API 的數據
        url1 = url_template1.format(date, stockNo)
        
        data1 = pd.read_html(StringIO(requests.get(url1).text))[0]
        data1.columns = data1.columns.droplevel(0)

        data1.insert(1, '股票代號', stockNo)
        
        # 加入每月營收
        head=int(str(date)[0:4])-1911
        month_in=int(str(date)[4:6])
        datee = f'{head}_{month_in}'
        url3 = url_template3.format(datee)
        res = requests.get(url3)
        res.encoding = 'big5'
        html_content = res.text
        try:
            html_df = pd.read_html(StringIO(html_content))
            apple=0
        except ValueError:
            # Handle the ValueError when no tables are found
            apple=1
            
        
        
        # Step3. 篩選出個股月營收資訊
        # 3.1 剃除行數錯誤的表格,並將表格合併
        month_df = pd.concat([df for df in html_df if df.shape[1] == 11]) 

    
        
        # 3.2 設定表格的 header 
        month_df.columns = month_df.columns.get_level_values(1)

        # 3.3 剃除多餘欄位, 重新排序索引值
        month_df = month_df[['公司 代號', '公司名稱', '當月營收']]
        month_df = month_df[month_df['公司名稱'] != '合計']
        month_df = month_df.reset_index(drop=True) 
        month_end_df = month_df.loc[month_df['公司 代號'] == stockNo]

        i=0
        key=0
        while len(month_end_df)==0:
            # If data for the current month is not available, fetch data for the previous month
            previous_month = dates[lastN_Year-1-i]  # Subtracting 100 from the date to get the previous month
            i+=1
            head=int(str(previous_month)[0:4])-1911
            month_in=int(str(previous_month)[4:6])
            datee = f'{head}_{month_in}'
            url3 = url_template3.format(datee)
            res = requests.get(url3)
            res.encoding = 'big5'
            html_content = res.text
            html_df = pd.read_html(StringIO(html_content))
            month_df = pd.concat([df for df in html_df if df.shape[1] == 11])
            month_df.columns = month_df.columns.get_level_values(1)
            month_df = month_df[['公司 代號', '公司名稱', '當月營收']]
            month_df = month_df[month_df['公司名稱'] != '合計']
            month_df = month_df.reset_index(drop=True) 
            month_end_df = month_df.loc[month_df['公司 代號'] == stockNo]
            if i == 4:
                data1.insert(1, '公司名稱', 'ETF')
                data1.insert(1, '當月營收', np.NAN)
                key=1
                break
            # Fetch data for the previous month
        if key==0:
            data1.insert(1, '公司名稱', month_end_df.iloc[0,1])
            data1.insert(1, '當月營收', month_end_df.iloc[0,2])
        i=0
        

        # 取得第二個 API 的數據
        url2 = url_template2.format(date, stockNo)
        # html_content = requests.get(url2).text
        data2 = pd.read_html(StringIO(requests.get(url2).text))[0]
        
        data2.columns = data2.columns.droplevel(0)

        

        data2[['年', '季']] = data2['財報年/季'].str.split('/', expand=True)
        # 下午3點跑的時候 有機率會data1/2 對不上(更新時間不一樣)
        if(data1.shape[0]!=data2.shape[0]):
            # 要出錯囉~~晚點再試，等表1表2更新就好...才怪
            # 檢查看誰比較長，把比較長的刪除一行看看
            if(max(data1.shape[0],data2.shape[0])==data2.shape[0]):
                data2 = data2.drop(data2.index[-1])
            elif(max(data1.shape[0],data2.shape[0])==data1.shape[0]):
                data1 = data1.drop(data1.index[-1])
        data2.iloc[:,0]=data1.iloc[:,0]

        # Ensure both DataFrames have a common column for merging
        common_column = '日期'

        
        
    # =============================================================================
    #     if common_column not in data1.columns or common_column not in data2.columns:
    #         raise ValueError(f"Column '{common_column}' not found in both DataFrames.")
    # 
    # =============================================================================
        # 合併兩份數據
        merged_data = pd.merge(data1, data2, on=common_column, how='outer')
    # 將合併後的數據添加到 all_data 中
        all_data = pd.concat([all_data, merged_data])

    # SQL_data 是新的欄位設定名稱

    SQL_data = {"日期": "ST_Date",  
                "股票代號": "SN_Code",
                "成交股數": "STe_TradeQuantity",
                "成交金額": "STe_TradeMoney",
                "開盤價": "STe_Open",
                "最高價": "STe_Max",
                "最低價": "STe_Min",
                "收盤價": "STe_Close",
                "漲跌價差": "STe_SpreadRatio",
                "成交筆數": "STe_TransActions",
                "殖利率(%)": "SB_Yield",
                "股利年度": "STe_Dividend_Year",
                "本益比": "SI_PE",
                "股價淨值比":"SB_PBRatio",
                "財報年/季":"ST_Year_Quarter",
                "年":"ST_Year",
                "季":"ST_Quarter",
                "公司名稱":"SN_Name",
                "當月營收":"SB_BussinessIncome"
                }


    # 新框架裡面放入原始資料
    SQL_data_df1 = pd.DataFrame(all_data)

    # 使用 rename 函數進行欄位名稱轉換
    SQL_data_df1.rename(columns=SQL_data, inplace=True)
    
    # SQL Server 連線部分
    server_name = 'localhost'
    database_name = 'StockProphet'
    username = 'sa'
    password = 'sa'

    connection_string = f'mssql+pyodbc://{server_name}/{database_name}?trusted_connection=yes&driver=ODBC+Driver+17+for+SQL+Server'
    engine = create_engine(connection_string)
    
    # 從資料庫讀取目標表格的資料到 DataFrame 中
    existing_data_df = pd.read_sql('SELECT * FROM Stock where SN_Code = '+stockNo, engine)
    struc = pd.DataFrame(columns=existing_data_df.columns)
    
    SQL_data_df = pd.merge(struc,SQL_data_df1,how='cross')
    for column in SQL_data_df1.columns:
        SQL_data_df[column] = SQL_data_df1[column].values
    
    # 改順序
    SQL_data_df = SQL_data_df[existing_data_df.columns]
    
    
    ## workdir
    # # 避免數值類的出現其他奇怪的字串，先全部轉為Nah
    SQL_data_df['STe_Close']=pd.to_numeric(SQL_data_df['STe_Close'], errors='coerce')
    SQL_data_df['STe_Open']=pd.to_numeric(SQL_data_df['STe_Open'], errors='coerce')
    SQL_data_df['STe_Max']=pd.to_numeric(SQL_data_df['STe_Max'], errors='coerce')
    SQL_data_df['STe_Min']=pd.to_numeric(SQL_data_df['STe_Min'], errors='coerce')
    SQL_data_df['STe_TradeQuantity']=pd.to_numeric(SQL_data_df['STe_TradeQuantity'], errors='coerce')
    SQL_data_df['STe_TradeMoney']=pd.to_numeric(SQL_data_df['STe_TradeMoney'], errors='coerce')
    SQL_data_df['SI_PE'] = pd.to_numeric(SQL_data_df['SI_PE'], errors='coerce')
    SQL_data_df['STe_TransActions']=pd.to_numeric(SQL_data_df['STe_TransActions'], errors='coerce')
    
    SQL_data_df['STe_Close'] = SQL_data_df['STe_Close'].fillna(SQL_data_df['STe_Close'].mean())
    SQL_data_df['STe_Open'] = SQL_data_df['STe_Open'].fillna(SQL_data_df['STe_Open'].mean())
    SQL_data_df['STe_Max'] = SQL_data_df['STe_Max'].fillna(SQL_data_df['STe_Max'].mean())
    SQL_data_df['STe_Min'] = SQL_data_df['STe_Min'].fillna(SQL_data_df['STe_Min'].mean())
    SQL_data_df['STe_TradeQuantity'] = SQL_data_df['STe_TradeQuantity'].fillna(SQL_data_df['STe_TradeQuantity'].mean())
    SQL_data_df['STe_TradeMoney'] = SQL_data_df['STe_TradeMoney'].fillna(SQL_data_df['STe_TradeMoney'].mean())
    SQL_data_df['SI_PE'] = SQL_data_df['SI_PE'].fillna(SQL_data_df['SI_PE'].mean())
    SQL_data_df['STe_TransActions'] = SQL_data_df['STe_TransActions'].fillna(SQL_data_df['STe_TransActions'].mean())

    # 指數五日平均線
    # 假設 SQL_data_df 是包含收盤價的 DataFrame
    # 使用當日的值填充空值
    # SQL_data_df['SI_MovingAverage_5'] = pd.to_numeric(SQL_data_df['STe_Close'].fillna(method='ffill').rolling(window=5).mean())
    SQL_data_df['SI_MovingAverage_5'] = pd.to_numeric(SQL_data_df['STe_Close'].ffill().rolling(window=5).mean(), errors='coerce')
    SQL_data_df['SI_MovingAverage_5'] = pd.to_numeric(SQL_data_df['SI_MovingAverage_5'], errors='coerce')

    # 指數三十日平均線
    # 使用當日的值填充前 29 天的空缺
    # SQL_data_df['SI_MovingAverage_30'] = SQL_data_df['STe_Close'].fillna(method='ffill').rolling(window=30).mean()
    SQL_data_df['SI_MovingAverage_30'] = SQL_data_df['STe_Close'].ffill().rolling(window=30).mean()
    SQL_data_df['SI_MovingAverage_30'] = pd.to_numeric(SQL_data_df['SI_MovingAverage_30'], errors='coerce')

    # RSV 五日線
    SQL_data_df['SI_RSV_5'] = ((SQL_data_df['STe_Close'] - SQL_data_df['STe_Close'].rolling(window=5).min())/(SQL_data_df['STe_Close'].rolling(window=5).max() - SQL_data_df['STe_Close'].rolling(window=5).min())) * 100
    SQL_data_df['SI_RSV_5'] = pd.to_numeric(SQL_data_df['SI_RSV_5'], errors='coerce')

    # RSV 三十日線
    SQL_data_df['SI_RSV_30'] = ((SQL_data_df['STe_Close'] - SQL_data_df['STe_Close'].rolling(window=30).min())/(SQL_data_df['STe_Close'].rolling(window=30).max() - SQL_data_df['STe_Close'].rolling(window=30).min())) * 100
    SQL_data_df['SI_RSV_30'] = pd.to_numeric(SQL_data_df['SI_RSV_30'], errors='coerce')

    # K 值五日線/三十日
    SQL_data_df['SI_K_5'] = SQL_data_df['SI_RSV_5'].copy()  # Create a copy of SI_RSV_5 as initial %K values
    SQL_data_df['SI_K_30'] = SQL_data_df['SI_RSV_30'].copy()  # Create a copy of SI_RSV_30 as initial %K values

    SQL_data_df=SQL_data_df.reset_index(drop=True)

    for i in range(1, len(SQL_data_df)):
        # Check if the previous day's %K value is NaN
        if pd.isna(SQL_data_df.at[i - 1, 'SI_K_5']):
            previous_k = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k = SQL_data_df.at[i - 1, 'SI_K_5']  # Use previous day's %K value
        if pd.isna(SQL_data_df.at[i - 1, 'SI_K_30']):
            previous_k_30 = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k_30 = SQL_data_df.at[i - 1, 'SI_K_30']  # Use previous day's %K value
        # Calculate the current day's %K using the modified formula
        current_k = (SQL_data_df.at[i, 'SI_RSV_5'] + previous_k * (5 - 1)) / 5
        current_k_30 = (SQL_data_df.at[i, 'SI_RSV_30'] + previous_k_30 * (5 - 1)) / 5
        # Update the %K value in the DataFrame
        SQL_data_df.at[i, 'SI_K_5'] = current_k
        SQL_data_df.at[i, 'SI_K_30'] = current_k_30

    # D 值五日線/三十日
    SQL_data_df['SI_D_5'] = SQL_data_df['SI_K_5'].copy()  # Create a copy of SI_RSV_5 as initial %K values
    SQL_data_df['SI_D_30'] = SQL_data_df['SI_K_30'].copy()  # Create a copy of SI_RSV_30 as initial %K values

    for i in range(1, len(SQL_data_df)):
        # Check if the previous day's %K value is NaN
        if pd.isna(SQL_data_df.at[i - 1, 'SI_D_5']):
            previous_k = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k = SQL_data_df.at[i - 1, 'SI_D_5']  # Use previous day's %K value
        if pd.isna(SQL_data_df.at[i - 1, 'SI_D_30']):
            previous_k_30 = 50  # Assign a value of 50 if previous day's %K is NaN
        else:
            previous_k_30 = SQL_data_df.at[i - 1, 'SI_D_30']  # Use previous day's %K value
        # Calculate the current day's %K using the modified formula
        current_k = (SQL_data_df.at[i, 'SI_K_5'] + previous_k * (5 - 1)) / 5
        current_k_30 = (SQL_data_df.at[i, 'SI_K_30'] + previous_k_30 * (5 - 1)) / 5
        # Update the %K value in the DataFrame
        SQL_data_df.at[i, 'SI_D_5'] = current_k
        SQL_data_df.at[i, 'SI_D_30'] = current_k_30


    # Define the period for EMA calculation
    ema_period = 12
    ema_period_long = 26

    # Calculate the first EMA value based on the average of 'STe_Close' for the first 7 days
    first_ema_value = SQL_data_df['STe_Close'].rolling(window=ema_period).mean()
    first_ema_value_long = SQL_data_df['STe_Close'].rolling(window=ema_period_long).mean()
    # Initialize the 'EMA' column with NaN values
    SQL_data_df['SI_ShortEMA'] = SQL_data_df['SI_LongEMA'] =  np.nan

    # Assign the calculated first EMA value to the first row of the 'EMA' column
    SQL_data_df.at[ema_period-1, 'SI_ShortEMA'] = first_ema_value.iloc[ema_period-1]
    SQL_data_df.at[ema_period_long-1, 'SI_LongEMA'] = first_ema_value_long.iloc[ema_period_long-1]
    # Calculate EMA for the following days using the specified formula
    for i in range(ema_period, len(SQL_data_df)):
        SQL_data_df.at[i, 'SI_ShortEMA'] = (2 * SQL_data_df.at[i, 'STe_Close'] / (ema_period + 1)) + ((ema_period - 1) * SQL_data_df.at[i - 1, 'SI_ShortEMA'] / (ema_period + 1))
        
    for i in range(ema_period_long, len(SQL_data_df)):
        SQL_data_df.at[i, 'SI_LongEMA']  = (2 * SQL_data_df.at[i, 'STe_Close'] / (ema_period_long+ 1)) + ((ema_period_long - 1) * SQL_data_df.at[i - 1, 'SI_LongEMA'] / (ema_period_long + 1))

    # Dif
    SQL_data_df['SI_Dif'] = SQL_data_df['SI_ShortEMA'] - SQL_data_df['SI_LongEMA']
    SQL_data_df['SI_Dif'] = pd.to_numeric(SQL_data_df['SI_Dif'], errors='coerce')

    # Define the period for MACD calculation
    macd_period = 9
    # Calculate the MACD

    SQL_data_df.at[ema_period_long+macd_period-2,'SI_MACD']=SQL_data_df['SI_Dif'][ema_period_long-1:ema_period_long+macd_period-1].sum()/9
        
    for i in range(ema_period_long+macd_period-1, len(SQL_data_df)):
        SQL_data_df.at[i, 'SI_MACD']=SQL_data_df.at[i - 1, 'SI_MACD']+2/(1+macd_period)*(SQL_data_df.at[i, 'SI_Dif']-SQL_data_df.at[i - 1, 'SI_MACD'])
        

    # OSC
    SQL_data_df['SI_OSC'] = ((SQL_data_df['STe_Close'] / SQL_data_df['STe_Close'].shift(9)) * 100) - 100
    SQL_data_df['SI_OSC'] = pd.to_numeric(SQL_data_df['SI_OSC'], errors='coerce')

    # MA
    SQL_data_df['SI_MA'] =  SQL_data_df['SI_OSC'].rolling(window=10).sum()/10
    SQL_data_df['SI_MA'] = pd.to_numeric(SQL_data_df['SI_MA'], errors='coerce')

    # EPS
    try:
        SQL_data_df['SB_EPS'] =  SQL_data_df['STe_Close'] / SQL_data_df['SI_PE']
        SQL_data_df['SB_EPS'] = pd.to_numeric(SQL_data_df['SB_EPS'], errors='coerce')
    except TypeError:
        # Handle the ValueError when no tables are found
        SQL_data_df['SB_EPS']=pd.to_numeric(0)




    # 將所有數據儲存到 SQL Server
    table_name = "Stock"
    SQL_data_df=SQL_data_df.reset_index(drop=True)
    
    for i in range(len(SQL_data_df)):
        SQL_data_df.at[i,'ST_Date']=str(int(SQL_data_df.iloc[i]['ST_Date'].replace('/',''))+19110000)        
        # SQL_data_df.at[i,'ST_Date'] = str(int(str(SQL_data_df.iloc[i][0]).replace('/',''))+19110000)
        if i==0:
            # SQL_data_df.at[i,'STe_SpreadRatio']=(SQL_data_df.iloc[i][7]-SQL_data_df.iloc[i][8])/(SQL_data_df.iloc[i][9]+float(SQL_data_df.iloc[i][8]))*100
            SQL_data_df.at[i,'STe_SpreadRatio'] = (SQL_data_df.iloc[i][7] - SQL_data_df.iloc[i][8]) / (SQL_data_df.iloc[i][9] + float(SQL_data_df.iloc[i][8])) * 100
        else:
            # SQL_data_df.at[i,'STe_SpreadRatio']=(SQL_data_df.iloc[i][7]-SQL_data_df.iloc[i][8])/SQL_data_df.iloc[i-1][9]*100
            SQL_data_df.at[i,'STe_SpreadRatio'] = (SQL_data_df.iloc[i][7] - SQL_data_df.iloc[i][8]) / SQL_data_df.iloc[i-1][9] * 100

    SQL_data_df['STe_SpreadRatio'] = pd.to_numeric(SQL_data_df['STe_SpreadRatio'], errors='coerce')

    # Replace NaN values with a default value (you can modify this based on your requirements)
    SQL_data_df['SI_MovingAverage_5'] = SQL_data_df['SI_MovingAverage_5'].fillna(SQL_data_df['STe_Close'])
    SQL_data_df['SI_MovingAverage_30'] = SQL_data_df['SI_MovingAverage_30'].fillna(SQL_data_df['STe_Close'])
    SQL_data_df['SI_RSV_5'] = SQL_data_df['SI_RSV_5'].fillna(SQL_data_df['SI_RSV_5'].mean())
    SQL_data_df['SI_RSV_30'] = SQL_data_df['SI_RSV_30'].fillna(SQL_data_df['SI_RSV_30'].mean())
    SQL_data_df['SI_K_5'] = SQL_data_df['SI_K_5'].fillna(SQL_data_df['SI_K_5'].mean())
    SQL_data_df['SI_K_30'] = SQL_data_df['SI_K_30'].fillna(SQL_data_df['SI_K_30'].mean())
    SQL_data_df['SI_D_5'] = SQL_data_df['SI_D_5'].fillna(SQL_data_df['SI_D_5'].mean())
    SQL_data_df['SI_D_30'] = SQL_data_df['SI_D_30'].fillna(SQL_data_df['SI_D_30'].mean())
    SQL_data_df['SI_ShortEMA'] = SQL_data_df['SI_ShortEMA'].fillna(SQL_data_df['SI_ShortEMA'].mean())
    SQL_data_df['SI_LongEMA'] = SQL_data_df['SI_LongEMA'].fillna(SQL_data_df['SI_LongEMA'].mean())
    SQL_data_df['SI_Dif'] = SQL_data_df['SI_Dif'].fillna(SQL_data_df['SI_Dif'].mean())
    SQL_data_df['SI_MACD'] = SQL_data_df['SI_MACD'].fillna(SQL_data_df['SI_MACD'].mean())
    SQL_data_df['SI_OSC'] = SQL_data_df['SI_OSC'].fillna(SQL_data_df['SI_OSC'].mean())
    SQL_data_df['SI_MA'] = SQL_data_df['SI_MA'].fillna(SQL_data_df['SI_MA'].mean())
    # SQL_data_df['S_PK'] = SQL_data_df['ST_Date'][0:4]+"-"+SQL_data_df['ST_Date'][4:6]+"-"+SQL_data_df['ST_Date'][6:8]+"_"+SQL_data_df['SN_Code']
    SQL_data_df['S_PK'] =  SQL_data_df['ST_Date'].str[0:4]+"-"+SQL_data_df['ST_Date'].str[4:6]+"-"+SQL_data_df['ST_Date'].str[6:8]+"_"+SQL_data_df['SN_Code']
    SQL_data_df['ST_Date']=  SQL_data_df['ST_Date'].str[0:4]+"-"+SQL_data_df['ST_Date'].str[4:6]+"-"+SQL_data_df['ST_Date'].str[6:8]
    SQL_data_df['ST_UpdateDate']= datetime.now().strftime("%Y-%m-%d")
    # 将'SI_PE'列中的值转换为数字，错误值将转换为0
    SQL_data_df['SI_PE'] = pd.to_numeric(SQL_data_df['SI_PE'], errors='coerce')

    # 计算除了NaN之外的平均值
    mean_without_nan = SQL_data_df['SI_PE'].dropna().mean()

    # 使用fillna将NaN填充为平均值
    SQL_data_df['SI_PE'] = SQL_data_df['SI_PE'].fillna(mean_without_nan)
    SQL_data_df['SI_PE'] = SQL_data_df['SI_PE'].fillna(0)


    

    
    # 找到目標表格中不存在的資料
    missing_data_df = pd.merge(SQL_data_df, existing_data_df[['S_PK']], on='S_PK', how='outer', indicator=True)
    missing_data_df = missing_data_df[missing_data_df['_merge'] == 'left_only'].drop(columns=['_merge'])
    missing_data_df.dropna(subset=['S_PK'], inplace=True)
    missing_data_df.drop_duplicates(subset=['S_PK'], inplace=True)
    


    # 如果有缺失的資料，將其插入到目標表格中
    if existing_data_df.empty:
        print("---資料庫沒值，全部灌進DB")
        SQL_data_df.to_sql(table_name, engine, if_exists='append', index=False)
        print("---成功")
    else:
        if not missing_data_df.empty:
            print("---資料庫有值，並且有差異，將差異灌進DB")
            missing_data_df.to_sql(table_name, engine, if_exists='append', index=False)
            print("---成功")
        else:
            print("---資料庫有值，但無差異，不動作")


def isoneTimeApi_call(stockNo):
     # SQL Server 連線部分
    server_name = 'localhost'
    database_name = 'StockProphet'
    username = 'sa'
    password = 'sa'

    connection_string = f'mssql+pyodbc://{server_name}/{database_name}?trusted_connection=yes&driver=ODBC+Driver+17+for+SQL+Server'
    engine = create_engine(connection_string)
    try:
    # 尝试连接数据库
        connection = engine.connect()
        print("-數據庫連接成功!")
        existing_data_df = pd.read_sql('SELECT * FROM Stock where SN_Code = '+stockNo, engine)
        connection.close()
        if(existing_data_df.shape[0]>0):
            print("-資料庫有資料，只呼叫API抓兩個月數據")
            return True
        else:
            print("-資料庫無資料，呼叫API抓一年的數據")
            return False
            
    except Exception as e:
        print("-數據庫連接失敗!", e)

#  Main----------------------------

# 使用者輸入股票代號和日期
stockNo = input("請輸入股票代號: ")
dates_input = input("請輸入日期(例如: 20230201,20230301): ")

isoneTimeApi = (isoneTimeApi_call(stockNo))
print('____________________________start_')
try:
    print("--Excute normal route")
    oneTimeApi_call(stockNo,dates_input) if isoneTimeApi else mutiTimesApi_call(stockNo,dates_input)
except:
    print("--Failed return mutiTimesApi_call")
    mutiTimesApi_call(stockNo,dates_input)
print('_end____________________________')

#  Main----------------------------

