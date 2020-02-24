using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaCalc
{
    class Program
    {
        static public CoronaParameter c = new CoronaParameter();
        static public GovernmentParameter g = new GovernmentParameter();
        static public List<CoronaPatient> p_list = new List<CoronaPatient>();
        static public System.Random r = new System.Random();

        static public int day = 0;
        static public int dead_count = 0;
        static public int complete_cure_count = 0;
        static public double mask_step_rate = 1.02;
        static public double contact_step_rate = 0.98;
        static public int yesterday_mask_sum = 0;
        static public int org_patient_number = 100;

        static public int in_hospital_sum = 0;

        static void Main(string[] args)
        {
            //-----コマンドライン処理-------------
            string[] cmds = System.Environment.GetCommandLineArgs();

            //パラメータ識別
            for (int i = 0; i < cmds.Length; i++)
            {
                if (cmds[i].Trim() == "/org_num")
                {

                    if (cmds.Length - 1 >= i + 1) //パラメータが存在するなら
                        org_patient_number = int.Parse(cmds[i + 1].Trim());

                    Console.WriteLine("Org. patient number: " + org_patient_number.ToString());

                }
                else if (cmds[i].Trim() == "/bed")
                {
                    if (cmds.Length - 1 >= i + 1) //パラメータが存在するなら
                        g.bed_count = int.Parse(cmds[i + 1].Trim());

                    Console.WriteLine("Hostipal bed count: " + g.bed_count.ToString());


                }
                else if (cmds[i].Trim() == "/dead_rate")
                {
                    if (cmds.Length - 1 >= i + 1) //パラメータが存在するなら
                        c.dead_rate = double.Parse(cmds[i + 1].Trim());

                    Console.WriteLine("Dead Rate: " + c.dead_rate);
                }

            }
            //-----コマンドライン処理-------------

            for (int i = 0; i < org_patient_number; i++)
            {
                CoronaPatient new_p = new CoronaPatient(r,c);
                new_p.id = p_list.Count;
                new_p.family_id = p_list.Count;
                new_p.care_level = 1; //感染されたばかりではなにもしない
                new_p.serious_factor = 0.1; //発症したばかりではひどくない
                p_list.Add(new_p);
            }

            Console.WriteLine("DAY:" + day.ToString() + " patient_number:" + p_list.Count);
            Console.WriteLine(" complete_cured:" + complete_cure_count.ToString() + " dead:" + dead_count.ToString());


            for (int j = 0; j < 100; j++)
            {
                int today_infect_sum = 0;
                int today_mask_sum = 0;
                int today_mask_predict = (int)(yesterday_mask_sum * mask_step_rate);
                double r0_mean = 0;
                int all_infected_number = 0;
                int care_level_4_count = 0;
                

                for (int i = 0; i < p_list.Count; i++)
                {

                    if (p_list[i].dead == 0 && p_list[i].complete_cure == 0)
                    {
                        List<CoronaPatient> new_list = p_list[i].infect(r, c);
                        dead_count += p_list[i].sick_step(r, c);
                        complete_cure_count += p_list[i].complete_cure_step();
                        today_infect_sum += p_list[i].use_mask;
                        p_list[i].contact_number = (int)(p_list[i].contact_number * contact_step_rate);

                        if (p_list[i].care_level >= 4)
                            care_level_4_count += 1; 

                        if (new_list.Count > 0)
                        {
                            today_infect_sum += new_list.Count;
                            //Console.WriteLine("ID:" + i.ToString() + " infected " + new_list.Count.ToString() + " peoples");
                            p_list.AddRange(new_list);
                        }
                    }



                    if (i < 100) //r0は死亡と関係ない
                        all_infected_number += p_list[i].infected_number;

                    r0_mean =(double)all_infected_number / 100;
                }

                if (care_level_4_count > g.bed_count)
                {
                    c.org_dead_rate = c.dead_rate;
                    c.dead_rate = c.dead_rate_no_bed;

                }
                else
                {
                    c.dead_rate = c.org_dead_rate;
                }

                in_hospital_sum = care_level_4_count;

                if (in_hospital_sum > g.bed_count)
                    in_hospital_sum = g.bed_count;

                c.left_bed_count = g.bed_count - in_hospital_sum;

                yesterday_mask_sum = today_mask_sum;

                day++;
                Console.WriteLine("---");
                Console.WriteLine("DAY:" + day.ToString() + " patient_number:" + p_list.Count + " in_hospital:" + in_hospital_sum.ToString());
                Console.WriteLine("infected:" + today_infect_sum.ToString() + " R0_mean:" + r0_mean.ToString("N2") + " cured:" + complete_cure_count.ToString() + " dead:" + dead_count.ToString());

                //list_patient(0, p_list.Count - 1);
                Console.ReadKey();
            }

        }

        static void list_patient(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                Console.WriteLine("---");
                Console.WriteLine("ID:" + p_list[i].id.ToString() + " AGE:" + p_list[i].age.ToString() + " SEX:" + p_list[i].gender.ToString() + " FAMILY_NUM:" + p_list[i].family_number.ToString());
                Console.WriteLine("CHECK:" + p_list[i].checkup.ToString() + " SERIOUS_FACTOR:" + p_list[i].serious_factor.ToString("N2") + " HEALTH_FACTOR:" + p_list[i].health_factor.ToString("N2"));
            }
        }
    }
}
